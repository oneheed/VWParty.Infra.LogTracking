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
- VWParty.Infra.LogTracking
- VWParty.Infra.LogTracking.Tests


# VWParty.Infra.LogTracking SDK �ϥλ���

�o�M SDK �ت��b�ѨM: ��V�A�Ȫ� Log Tracking �̤j����ê�b��S���Τ@�� KEY �Ӱl�d�Y�ӥ��Ȧb���P�A�Ȥ������B�z�L�{�C
�]���]�p�o�M SDK�A�γ�@�@�� Request ID �Ӧ�_�Ҧ��������C���F�F��o�ӥت��A���X�ӹ�@�W�����e�ݭn�J�A:

1. ��ɲ��� Request ID ?
2. Request ID �p��ǻ���U�@�ӪA�� ?
3. �o�M�����_�� Logger ��X ?
4. �ɤJ�o�M����O�_�ݭn�j�T��g�J���� Application ?

�]���A������ SDK �N�]�p�F�o�M����A�����B�z���V�A�Ȱl�� Log �������Ҧ����D:


## LogTrackerContext

```LogTrackerContext``` �O�o�M SDK ���֤߾���CContext ����N��F�ثe LogTracking �������T (�ثe��@�������T�����:
Request-ID �P Request-Start-TimeUTC)�C�u�n�ॿ�T�x�� LogTrackerContext, �Ҧ��� Log ����N�ॿ�T����X Log, �ƫ�N�ॿ�T��
�٭�Y�ӥ��Ȧb�Ҧ����A�Ȥ��B�z���ԲӰO���C

LogTrackerContext �u��z�L .Create() ��k�ӫإߡC�إߪ��ʧ@�|���ͤ@���s�� unique id �@�� Request-ID, �]�|��ثe���ɶ� (UTC)
�@�_�O���U�ӡA��K�N�ӥΥ��ȱҰʨ��������۹�ɶ��Ӱl�d�T���C

LogTrackerContext �إ� (Create) ��A�}�o�̥��������p�M�w�p��⥦�O�s�U�ӡA����L code ����T�a���? �o�ǫO�s���覡�w�q�A
StorageTypeEnum, �ثe���w�q���x�s�覡��:

- NONE, ���x�s�A�Ѷ}�o�H���ۦ�O�d�P�ǻ�
- HttpContext, �x�s�b HttpContext �� Request Header ��, �u�� ASP.NET ���ε{�����ϥΡC�u�n�b�P�@�� Http Request �� pipeline ������s���C
- OwinContext, �x�s�b OwinContext �� Request �� (�ثe�|����@)
- ThreadDataSlot, �x�s�b�ثe�� thread �M���x�s�Ŷ����C�u�n�b�P�@�� thread �H�U������s���C�Y�{������|��V���P threads, �h������ʦ걵�ಾ

��V���P�� Context, �h�������T���ಾ�ثe�� LogTrackerContext. �p�G�A�Q�걵�e�����d�ǻ��L�Ӫ� LogTrackerContext, �Х� Init() �өӱ�
�e���� Context, �åB�⥦�x�s�b�X�A�� Storage ���C


������Ϊ����p�U�A�D�n�N�O�Ҽ{���Ʊ�:
1. ��ɭn���� Context ?
2. �n�p��걵 Context ?

�H�U���O�����o��Ӱʧ@:

### ��ɲ��� Context ?

�ثe��M����A���X�ӾA�X���� Context ���ɾ�:

1. �g�L API Gateway �ɦ۰ʫإ� (�w����)
�u�n�O�z�L API Gateway ��e�� API Call, ���|�۰ʦb Header ���s�� Create �᪺ Context �����T�C
2. WebAPI �M�� LogTrackerAttribute, �ĥΦp�P API Gateway, �g�L Controller �N�|�۰ʫإ�
3. ��L�A�Ѷ}�o�̦ۦ�I�s .Create( ) �إ�


�ǻ�������A�ثe SDK �]�ǳƤF�X�ӱ`�Ϊ��޹D�A�Q��Τ@�B�z:

1. HttpClient Handler - �Y�A�z�L HttpClient �I�s WebAPI, �z�L HttpClient Handler �N��۰ʦa��ثe Context �z�L Request Headers ��e��U�@���C
2. ASP.NET MVC Filter - �Y�e�ݤw�g�z�L Request Header �ǻ� Context, �h�u�n�аO Filter Attribute, �N��۰ʩӱ��Ӧ� Request Headers �� Context, �Y�L�h�|�ۤv���ͤ@�աC�åB�A API �I�s���e����O�g�U�@�� Log
(��: �A���ݭn�z�L Filter, �]��z�L Context.Current ���o�W�@���ǻ��L�Ӫ� Context)

���F��K�b��x�̭��Х� Context ����T�ASDK �]���F�U�C��X�P�B�z:

1. ���X�J���� Mnemosyne Logger, �z�L Logger ��X�� GrayLog �������A���|�۰ʪ��[�ثe���Ҫ� request-id, request-start-time, request-execute-time
2. �Y�n�z�L NLog ��X�A�h SDK �]���ѤF renderer: ${...}



# �d��

(USAGE)
