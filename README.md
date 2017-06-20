[![build status](https://gitlab.66.net/msa/VWParty.Infra.LogTracking/badges/develop/build.svg)](https://gitlab.66.net/msa/VWParty.Infra.LogTracking/commits/develop) (branch: develop)
----

# �p��z�L NuGet ���o�o�ӮM��?

�M�󪺵o�泣�z�L���q�� NuGet server �i��C�o�ӱM�פ]�w���T�]�w�n CI / CD �y�{�C�u�n�{���X���e (push)
�� GitLab �N�|Ĳ�o CI-Runner, �۰ʽsĶ�Υ��]�M��A���e�� NuGet server.

���q�� NuGet Server URL:

develop branch: �۰ʵo�� BETA ���� (pre-release)
master branch:  �۰ʵo�楿���� (release)


�p����o�̷s�� (release)
```powershell
install-package VWParty.Infra.LogTracking
```

�p����o�̷s�� (pre-release, beta)
```powershell
install-package VWParty.Infra.LogTracking -pre
```


# Solution ����

�o�� repository �]�t�@�� visual studio solution, �̭����C�� project �γ~�����p�U:

- POC.Client
- POC.WebAPI1
- POC.WebAPI2

> POC �d�һ����ADemo �� Client �o�� Http Request , �I�s WebAPI1 ���Ѫ� REST API
> WebAPI1 ���� request ��A�A��o Http Request �� WebAPI2�C
> �ǵ۳o�ˤ@�s��I�s���L�{�A�i�� LogTrackerContext ���B�@�覡

- VWParty.Infra.LogTracking
> SDK �M�ץ���

- VWParty.Infra.LogTracking.Tests
> SDK ���椸����



# VWParty.Infra.LogTracking SDK �ϥλ���

�o�M SDK �ت��b�ѨM: ��V�A�Ȫ� Log Tracking �̤j����ê�b��S���Τ@�� KEY �Ӱl�d�Y�ӥ��Ȧb���P�A�Ȥ������B�z�L�{�C
�]���]�p�o�M SDK�A�γ�@�@�� Request ID �Ӧ�_�Ҧ��������C���F�F��o�ӥت��A���X�ӹ�@�W�����e�ݭn�J�A:

1. ��ɲ��� Request ID ?
2. Request ID �p��ǻ���U�@�ӪA�� ?
3. �o�M�����_�� Logger ��X ?
4. �ɤJ�o�M����O�_�ݭn�j�T��g�J���� Application ?

�]���A������ SDK �N�]�p�F�o�M����A�����B�z���V�A�Ȱl�� Log �������Ҧ����D:


## LogTrackerContext

```LogTrackerContext``` �O�o�M SDK ���֤߾���C```LogTrackerContext``` ����N��F�ثe Log Tracking �������T (�ثe��@�������T�����:
Request-ID �P Request-Start-TimeUTC)�C�u�n�ॿ�T�x�� ```LogTrackerContext```, �Ҧ��� Log ����N�ॿ�T����X Log, �ƫ�N�ॿ�T��
�٭�Y�ӥ��Ȧb�Ҧ����A�Ȥ��B�z���ԲӰO���C

```LogTrackerContext``` �u��z�L ```LogTrackerContext.Create()``` ��k�ӫإߡC�إߪ��ʧ@�|���ͤ@���s�� unique id �@�� Request-ID, �]�|��ثe���ɶ� (UTC)
�@�_�O���U�ӡA��K�N�ӥΥ��ȱҰʨ��������۹�ɶ��Ӱl�d�T���C

```LogTrackerContext``` �إ� (```Create```) ��A�}�o�̥��������p�M�w�p��⥦�O�s�U�ӡA����L code ����T�a���? �o�ǫO�s���覡�w�q�b
```LogTrackerContextStorageTypeEnum```, �ثe���w�q���x�s�覡��:

```csharp
    public enum LogTrackerContextStorageTypeEnum : int
    {
        ASPNET_HTTPCONTEXT,
        OWIN_CONTEXT,   // not supported
        THREAD_DATASLOT,
        NONE
    }
```
- ```NONE```, ���x�s�A�Ѷ}�o�H���ۦ�O�d�P�ǻ�
- ```ASPNET_HTTPCONTEXT```, �x�s�b ```HttpContext``` �� Request Header ��, �u�� ASP.NET ���ε{�����ϥΡC�u�n�b�P�@�� Http Request �� pipeline ������s���C
- ```OWIN_CONTEXT```, �x�s�b ```OwinContext``` �� Request �� (�ثe�|����@)
- ```THREAD_DATASLOT```, �x�s�b�ثe�� thread �M���x�s�Ŷ����C�u�n�b�P�@�� thread �H�U������s���C�Y�{������|��V���P threads, �h������ʦ걵�ಾ

��V���P�� Context, �h�������T���ಾ�ثe�� ```LogTrackerContext```. �p�G�A�Q�걵�e�����d�ǻ��L�Ӫ� ```LogTrackerContext```, �Х� ```LogTrackerContext.Init()``` �өӱ�
�e���� Context, �åB�⥦�x�s�b�X�A�� Storage ���C


������Ϊ����p�U�A�D�n�N�O�Ҽ{���Ʊ�:
1. ��ɭn���� ```LogTrackerContext``` ?
2. �n�p��걵 ```LogTrackerContext``` ?

�H�U���O�����o��Ӱʧ@:

### ��ɲ��� Context ?

�ثe��M����A���X�ӾA�X���� ```LogTrackerContext``` ���ɾ�:

1. �g�L API Gateway �ɦ۰ʫإ� (�w����)
�u�n�O�z�L API Gateway ��e�� API Call, ���|�۰ʦb Header ���s�� ```LogTrackerContext``` �����T�C
2. WebAPI �M�� ```LogTrackerAttribute```, �ĥΦp�P API Gateway, �g�L ```Controller``` �N�|�۰ʫإ�
3. ��L�A�Ѷ}�o�̦ۦ�I�s ```LogTrackerContext.Create()``` �إ�


�ǻ�������A�ثe SDK �]�ǳƤF�X�ӱ`�Ϊ��޹D�A�Q��Τ@�B�z:

1. HttpClient Handler - �Y�A�z�L ```HttpClient``` �I�s WebAPI, �z�L HttpClient Handler �N��۰ʦa��ثe ```LogTrackerContext``` �z�L Request Headers ��e��U�@���C
2. ASP.NET MVC Filter - �Y�e�ݤw�g�z�L Request Header �ǻ� ```LogTrackerContext```, �h�u�n�аO Filter Attribute, �N��۰ʩӱ��Ӧ� Request Headers �� ```LogTrackerContext```, �Y�L�h�|�ۤv���ͤ@�աC�åB�A API �I�s���e����O�g�U�@�� Log
(��: �A���ݭn�z�L Filter, �]��z�L ```LogTrackerContext.Current``` ���o�W�@���ǻ��L�Ӫ� ```LogTrackerContext```)

���F��K�b��x�̭��Х� ```LogTrackerContext``` ����T�ASDK �]���F�U�C��X�P�B�z:

1. ���X�J���� Mnemosyne Logger, �z�L Logger ��X�� GrayLog �������A���|�۰ʪ��[�ثe���Ҫ� request-id, request-start-time, request-execute-time
2. �Y�n�z�L NLog ��X�A�h SDK �]���ѤF renderer: ${vwparty-request-id} ���C�Ҧp:
```xml
    <variable name="Layout" value="${longdate} (${vwparty-request-id},${vwparty-request-time},${vwparty-request-execute}) | ${message} ${newline}"/>
```




## �d��: Create() - �إߤ@�շs�� LogTrackerContext (�n�l�ܨƥ󪺰_�l�I)

���`���p�U�AContext �ݭn�������T (request-id + start-time) ���|�b API Gateway ���q�N�ǳƦn�C���O�����������p�ڭ̻ݭn��ʲ���
�o�Ǹ�T�C���o�ػݨD�ɡA�ݭn�� ```LogTrackerContext.Create()``` �Ӷi��:

```csharp
// ���ͤ@�շs�� context, �u�Ǧ^����, ���x�s�b���� storage
var context = LogTrackerContext.Create("TEMP-HC", LogTrackerContextStorageTypeEnum.NONE);
```

�Y�A���T�����D�A�Q�n�x�s context ���覡���ܡA�i�H�������w�C�U�C���椸���դ��q�i�H�M����F�o�ӷ���:

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



## �d��: Init() - ��V���ҮɡA�n�ӱ��e�@�����Ҷǻ��L�Ӫ� LogTrackerContext

�Y�A�w�g�q��L�޹D���o context ����������T�A�ݭn���s Init �ثe�� context ���Ҫ��ܡA�i�H�Ѧҳo�q code ���@�k:

```csharp

string current_request_id = "DEMO-1234567890";		// ���o�ثe�� request id
DateTime current_request_time = DateTime.UtcNow;		// ���o�ثe�� request start time

// �b���w�� storage �W�� Init context
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


�p�G�A�w�g�q�O���޹D�������� context ����A�h�o�B�J�i�H²�Ƭ�:

```csharp

var previous_context = ...; // ���o���e�� context ����

// �b���w�� storage �W�� Init context
LogTrackerContext context = LogTrackerContext.Init(
    LogTrackerContextStorageTypeEnum.THREAD_DATASLOT,
	previous_context);

Console.WriteLine(
    "TID: {0}, Request-ID: {1}, Request-Time: {2}", 
    Thread.CurrentThread.ManagedThreadId, 
    context.RequestId, 
    context.RequestStartTimeUTC);
```


## ���Υثe�� LogTrackerContext

�p�G�ثe���Ҫ� context ���w���`�� init, ����n���o�L�O�ܮe�����A�u�n�H�ɳz�L ```LogTrackerContext.Current``` �N��������� context �F�C

```csharp
Console.WriteLine(
    "TID: {0}, Request-ID: {1}, Request-Time: {2}", 
    Thread.CurrentThread.ManagedThreadId, 
    LogTrackerContext.Current.RequestId, 
    LogTrackerContext.Current.RequestStartTimeUTC);
```

�䤤, RequestExecutingTime �O�Y�ɭp�⪺�A�A�i�H�H�ɩI�s�L���o context �Q�إ� (create) ���{�b�j�F�h�֮ɶ��C

```csharp
Console.WriteLine("Execute Time: {0}", LogTrackerContext.Current.RequestExecutingTime);
```


## HttpClient Handler

�Y�A�ݭn�z�L HttpClient �s����L�� WebAPI, �P�ɧƱ��ثe�� context �ǻ��U�h�A����i�H�Ѧ� /POC/POC.Client �o�ӽd��:

```csharp
    HttpClient client = new HttpClient(new LogTrackerHandler());
    client.BaseAddress = new Uri("http://localhost:31554/");
    Console.WriteLine(client.GetAsync("/api/values").Result);
    Console.WriteLine(client.GetAsync("/api/values/123").Result);
```
LogTrackerHandler �|�� HttpClient �إߤ@�ձM�ݪ� context, �åB�b���᪺�⦸�I�s���ΦP�@�� context �ǻ��U�h�C�N��
��� WebAPI �������N�i�H�l�ܨ�P�@�� request id�C



## ASP.NET MVC Filter

## NLog Extension

## GrayLog Logger

