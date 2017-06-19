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
- VWParty.Infra.LogTracking
- VWParty.Infra.LogTracking.Tests


# VWParty.Infra.LogTracking SDK 使用說明

這套 SDK 目的在解決: 跨越服務的 Log Tracking 最大的障礙在於沒有統一的 KEY 來追查某個任務在不同服務之間的處理過程。
因此設計這套 SDK，用單一一個 Request ID 來串起所有的紀錄。為了達到這個目的，有幾個實作上的門檻需要克服:

1. 何時產生 Request ID ?
2. Request ID 如何傳遞到下一個服務 ?
3. 這套機制能否跟 Logger 整合 ?
4. 導入這套機制是否需要大幅改寫既有的 Application ?

因此，對應的 SDK 就設計了這套機制，集中處理跟跨越服務追蹤 Log 相關的所有問題:


## LogTrackerContext

```LogTrackerContext``` 是這套 SDK 的核心機制。Context 物件代表了目前 LogTracking 的關鍵資訊 (目前實作的關鍵資訊有兩個:
Request-ID 與 Request-Start-TimeUTC)。只要能正確掌握 LogTrackerContext, 所有的 Log 機制就能正確的輸出 Log, 事後就能正確的
還原某個任務在所有的服務中處理的詳細記錄。

LogTrackerContext 只能透過 .Create() 方法來建立。建立的動作會產生一筆新的 unique id 作為 Request-ID, 也會把目前的時間 (UTC)
一起記錄下來，方便將來用任務啟動那瞬間的相對時間來追查訊息。

LogTrackerContext 建立 (Create) 後，開發者必須視情況決定如何把它保存下來，讓其他 code 能明確地找到? 這些保存的方式定義再
StorageTypeEnum, 目前有定義的儲存方式有:

- NONE, 不儲存，由開發人員自行保留與傳遞
- HttpContext, 儲存在 HttpContext 的 Request Header 內, 只限 ASP.NET 應用程式內使用。只要在同一個 Http Request 的 pipeline 內都能存取。
- OwinContext, 儲存在 OwinContext 的 Request 內 (目前尚未實作)
- ThreadDataSlot, 儲存在目前的 thread 專屬儲存空間內。只要在同一個 thread 以下都能夠存取。若程式執行會跨越不同 threads, 則必須手動串接轉移

跨越不同的 Context, 則必須明確的轉移目前的 LogTrackerContext. 如果你想串接前面關卡傳遞過來的 LogTrackerContext, 請用 Init() 來承接
前面的 Context, 並且把它儲存在合適的 Storage 內。


實際應用的狀況下，主要就是考慮兩件事情:
1. 何時要產生 Context ?
2. 要如何串接 Context ?

以下分別說明這兩個動作:

### 何時產生 Context ?

目前整套機制，有幾個適合產生 Context 的時機:

1. 經過 API Gateway 時自動建立 (已完成)
只要是透過 API Gateway 轉送的 API Call, 都會自動在 Header 內存放 Create 後的 Context 關鍵資訊。
2. WebAPI 套用 LogTrackerAttribute, 效用如同 API Gateway, 經過 Controller 就會自動建立
3. 其他，由開發者自行呼叫 .Create( ) 建立


傳遞的機制，目前 SDK 也準備了幾個常用的管道，利於統一處理:

1. HttpClient Handler - 若你透過 HttpClient 呼叫 WebAPI, 透過 HttpClient Handler 就能自動地把目前 Context 透過 Request Headers 轉送到下一關。
2. ASP.NET MVC Filter - 若前端已經透過 Request Header 傳遞 Context, 則只要標記 Filter Attribute, 就能自動承接來自 Request Headers 的 Context, 若無則會自己產生一組。並且再 API 呼叫的前後分別寫下一筆 Log
(註: 你不需要透過 Filter, 也能透過 Context.Current 取得上一關傳遞過來的 Context)

為了方便在日誌裡面標示 Context 的資訊，SDK 也做了下列整合與處理:

1. 結合既有的 Mnemosyne Logger, 透過 Logger 輸出到 GrayLog 的紀錄，都會自動附加目前環境的 request-id, request-start-time, request-execute-time
2. 若要透過 NLog 輸出，則 SDK 也提供了 renderer: ${...}



# 範例

(USAGE)
