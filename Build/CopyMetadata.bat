:: �����Ը��Ĺ���Ԫ���ݸ��Ʋ��浵
xcopy ����������ָ��.url Publish\����������ָ��.url /e /y
@echo ����Ԫ���ݵ������ļ���
xcopy Debug\net6.0-windows10.0.18362.0\Metadata Publish\Metadata /e /y
@echo ����Ԫ���ݵ��浵�ļ���
xcopy Debug\net6.0-windows10.0.18362.0\Metadata ..\Metadata /e /y