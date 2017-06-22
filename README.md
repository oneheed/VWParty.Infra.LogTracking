[![build status](https://gitlab.66.net/msa/VWParty.Infra.LogTracking/badges/develop/build.svg)](https://gitlab.66.net/msa/VWParty.Infra.LogTracking/commits/develop) (branch: develop)
----

# 如何透過 NuGet 取得這個套件?

套件的發行都透過公司的 NuGet server 進行。這個專案也已正確設定好 CI / CD 流程。只要程式碼推送 (push)
到 GitLab 就會觸發 CI-Runner, 自動編譯及打包套件，推送到 NuGet server.

公司的 NuGet Server URL:

develop branch: 自動發行 BETA 版本 (pre-release)
master branch:  自動發行正式版 (release)


如何取得最新版 (release)
```powershell
install-package VWParty.Infra.LogTracking
```

如何取得最新版 (pre-release, beta)
```powershell
install-package VWParty.Infra.LogTracking -pre
```


# Solution 說明

這個 repository 包含一個 visual studio solution, 裡面的每個 project 用途說明如下:

- POC.Client
- POC.WebAPI1
- POC.WebAPI2

> POC 範例說明，Demo 由 Client 發動 Http Request , 呼叫 WebAPI1 提供的 REST API
> WebAPI1 接到 request 後，再轉發 Http Request 給 WebAPI2。
> 藉著這樣一連串呼叫的過程，展示 LogTrackerContext 的運作方式

- VWParty.Infra.LogTracking
> SDK 專案本身

- VWParty.Infra.LogTracking.Tests
> SDK 的單元測試



# VWParty.Infra.LogTracking SDK 使用說明

這套 SDK 目的在解決: 跨越服務的 Log Tracking 最大的障礙在於沒有統一的 KEY 來追查某個任務在不同服務之間的處理過程。
因此設計這套 SDK，用單一一個 Request ID 來串起所有的紀錄。為了達到這個目的，有幾個實作上的門檻需要克服:

1. 何時產生 Request ID ?
2. Request ID 如何傳遞到下一個服務 ?
3. 這套機制能否跟 Logger 整合 ?
4. 導入這套機制是否需要大幅改寫既有的 Application ?

因此，對應的 SDK 就設計了這套機制，集中處理跟跨越服務追蹤 Log 相關的所有問題:


## LogTrackerContext

```LogTrackerContext``` 是這套 SDK 的核心機制。```LogTrackerContext``` 物件代表了目前 Log Tracking 的關鍵資訊 (目前實作的關鍵資訊有兩個:
Request-ID 與 Request-Start-TimeUTC)。只要能正確掌握 ```LogTrackerContext```, 所有的 Log 機制就能正確的輸出 Log, 事後就能正確的
還原某個任務在所有的服務中處理的詳細記錄。

```LogTrackerContext``` 只能透過 ```LogTrackerContext.Create()``` 方法來建立。建立的動作會產生一筆新的 unique id 作為 Request-ID, 也會把目前的時間 (UTC)
一起記錄下來，方便將來用任務啟動那瞬間的相對時間來追查訊息。

```LogTrackerContext``` 建立 (```Create```) 後，開發者必須視情況決定如何把它保存下來，讓其他 code 能明確地找到? 這些保存的方式定義在
```LogTrackerContextStorageTypeEnum```, 目前有定義的儲存方式有:

```csharp
    public enum LogTrackerContextStorageTypeEnum : int
    {
        ASPNET_HTTPCONTEXT,
        OWIN_CONTEXT,   // not supported
        THREAD_DATASLOT,
        NONE
    }
```
- ```NONE```, 不儲存，由開發人員自行保留與傳遞
- ```ASPNET_HTTPCONTEXT```, 儲存在 ```HttpContext``` 的 Request Header 內, 只限 ASP.NET 應用程式內使用。只要在同一個 Http Request 的 pipeline 內都能存取。
- ```OWIN_CONTEXT```, 儲存在 ```OwinContext``` 的 Request 內 (目前尚未實作)
- ```THREAD_DATASLOT```, 儲存在目前的 thread 專屬儲存空間內。只要在同一個 thread 以下都能夠存取。若程式執行會跨越不同 threads, 則必須手動串接轉移

跨越不同的 Context, 則必須明確的轉移目前的 ```LogTrackerContext```. 如果你想串接前面關卡傳遞過來的 ```LogTrackerContext```, 請用 ```LogTrackerContext.Init()``` 來承接
前面的 Context, 並且把它儲存在合適的 Storage 內。


實際應用的狀況下，主要就是考慮兩件事情:
1. 何時要產生 ```LogTrackerContext``` ?
2. 要如何串接 ```LogTrackerContext``` ?

以下分別說明這兩個動作:

### 何時產生 Context ?

目前整套機制，有幾個適合產生 ```LogTrackerContext``` 的時機:

1. 經過 API Gateway 時自動建立 (已完成)
只要是透過 API Gateway 轉送的 API Call, 都會自動在 Header 內存放 ```LogTrackerContext``` 關鍵資訊。
2. WebAPI 套用 ```LogTrackerAttribute```, 效用如同 API Gateway, 經過 ```Controller``` 就會自動建立
3. 其他，由開發者自行呼叫 ```LogTrackerContext.Create()``` 建立


傳遞的機制，目前 SDK 也準備了幾個常用的管道，利於統一處理:

1. HttpClient Handler - 若你透過 ```HttpClient``` 呼叫 WebAPI, 透過 HttpClient Handler 就能自動地把目前 ```LogTrackerContext``` 透過 Request Headers 轉送到下一關。
2. ASP.NET MVC Filter - 若前端已經透過 Request Header 傳遞 ```LogTrackerContext```, 則只要標記 Filter Attribute, 就能自動承接來自 Request Headers 的 ```LogTrackerContext```, 若無則會自己產生一組。並且再 API 呼叫的前後分別寫下一筆 Log
(註: 你不需要透過 Filter, 也能透過 ```LogTrackerContext.Current``` 取得上一關傳遞過來的 ```LogTrackerContext```)

為了方便在日誌裡面標示 ```LogTrackerContext``` 的資訊，SDK 也做了下列整合與處理:

1. 結合既有的 Mnemosyne Logger, 透過 Logger 輸出到 GrayLog 的紀錄，都會自動附加目前環境的 request-id, request-start-time, request-execute-time
2. 若要透過 NLog 輸出，則 SDK 也提供了 renderer: ${vwparty-request-id} 等。例如:
```xml
    <variable name="Layout" value="${longdate} (${vwparty-request-id},${vwparty-request-time},${vwparty-request-execute}) | ${message} ${newline}"/>
```




## 範例: Create() - 建立一組新的 LogTrackerContext (要追蹤事件的起始點)

正常情況下，Context 需要的關鍵資訊 (request-id + start-time) 都會在 API Gateway 階段就準備好。但是仍有部分狀況我們需要手動產生
這些資訊。有這種需求時，需要用 ```LogTrackerContext.Create()``` 來進行:

```csharp
// 產生一組新的 context, 只傳回物件, 不儲存在任何 storage
var context = LogTrackerContext.Create("TEMP-HC", LogTrackerContextStorageTypeEnum.NONE);
```

若你明確的知道你想要儲存 context 的方式的話，可以直接指定。下列的單元測試片段可以清楚表達這個概念:

```csharp
        public void Test_BasicThreadDataSlotStorage()
        {
            var context = LogTrackerContext.Create("UNITTEST", LogTrackerContextStorageTypeEnum.THREAD_DATASLOT);
            Assert.AreEqual(
                context.RequestId,
                LogTrackerContext.Current.RequestId);
            Assert.AreEqual(
                context.RequestStartTimeUTC,
                LogTrackerContext.Current.RequestStartTimeUTC);
        }
```



## 範例: Init() - 跨越環境時，要承接前一個環境傳遞過來的 LogTrackerContext

若你已經從其他管道取得 context 的兩個關鍵資訊，需要重新 Init 目前的 context 環境的話，可以參考這段 code 的作法:

```csharp

string current_request_id = "DEMO-1234567890";		// 取得目前的 request id
DateTime current_request_time = DateTime.UtcNow;		// 取得目前的 request start time

// 在指定的 storage 上面 Init context
LogTrackerContext context = LogTrackerContext.Init(
    LogTrackerContextStorageTypeEnum.THREAD_DATASLOT,
    current_request_id,
    current_request_time);

Console.WriteLine(
  "TID: {0}, Request-ID: {1}, Request-Time: {2}", 
  Thread.CurrentThread.ManagedThreadId, 
  context.RequestId, 
  context.RequestStartTimeUTC);

```


如果你已經從別的管道直接拿到 context 物件，則這步驟可以簡化為:

```csharp

var previous_context = ...; // 取得先前的 context 物件

// 在指定的 storage 上面 Init context
LogTrackerContext context = LogTrackerContext.Init(
    LogTrackerContextStorageTypeEnum.THREAD_DATASLOT,
	previous_context);

Console.WriteLine(
    "TID: {0}, Request-ID: {1}, Request-Time: {2}", 
    Thread.CurrentThread.ManagedThreadId, 
    context.RequestId, 
    context.RequestStartTimeUTC);
```


## 取用目前的 LogTrackerContext

如果目前環境的 context 都已正常的 init, 那麼要取得他是很容易的，只要隨時透過 ```LogTrackerContext.Current``` 就能夠拿的到 context 了。

```csharp
Console.WriteLine(
    "TID: {0}, Request-ID: {1}, Request-Time: {2}", 
    Thread.CurrentThread.ManagedThreadId, 
    LogTrackerContext.Current.RequestId, 
    LogTrackerContext.Current.RequestStartTimeUTC);
```

其中, RequestExecutingTime 是即時計算的，你可以隨時呼叫他取得 context 被建立 (create) 後到現在隔了多少時間。

```csharp
Console.WriteLine("Execute Time: {0}", LogTrackerContext.Current.RequestExecutingTime);
```


## HttpClient Handler

若你需要透過 HttpClient 存取其他的 WebAPI, 同時希望把目前的 context 傳遞下去，那麼可以參考 /POC/POC.Client 這個範例:

```csharp
    HttpClient client = new HttpClient(new LogTrackerHandler());
    client.BaseAddress = new Uri("http://localhost:31554/");
    Console.WriteLine(client.GetAsync("/api/values").Result);
    Console.WriteLine(client.GetAsync("/api/values/123").Result);
```
LogTrackerHandler 會替 HttpClient 建立一組專屬的 context, 並且在之後的兩次呼叫都用同一組 context 傳遞下去。將來
兩個 WebAPI 的紀錄就可以追蹤到同一筆 request id。



## ASP.NET MVC Filter

若你是開發 ASP.NET MVC 或 WebAPI 應用程式, 可參考 POC.WebAPI1 或 POC.WebAPI2 透過以下方式, 承接呼叫端傳送過來的 LogTrackerContext, 
若呼叫端未傳送LogTracerContext, 則自行產生一組新的LogTrackerContext, 範例如下:

```csharp
    [LogTracker(Prefix = "POC1")]
    public class ValuesController : ApiController
    {
        ...
    }
```

## NLog Extension

## GrayLog Logger

