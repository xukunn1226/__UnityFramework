@echo off
:LOOP
    :: ʹ�÷�ʽ�����ű�ֱ�ӷŵ�C:\Users\lin\AppData\Roaming\Microsoft\Windows\SendToĿ¼�£���������ͨ���Ҽ�AB����������͵��� ѡ���Ӧ�Ľű�
    rem ǰ���ǽ��·���webExtractPath��binary2testPath������unity·���滻���㱾��unity��װĿ¼

    set webExtractPath="D:\Program Files\2019.3.3f1\Editor\Data\Tools\WebExtract.exe"
    set binary2testPath="D:\Program Files\2019.3.3f1\Editor\Data\Tools\binary2text.exe"
    set /p filePath="������Bundle Path: "
    set transitionFolder=%filePath%_data
	
    if not exist %webExtractPath% (
        echo ������%webExtractPath%
        goto END
    )	
	
    if not exist %binary2testPath% (
        echo ������%binary2testPath%
        goto END
    )
	
    if %filePath%! == ! (
        goto END
    )
    call %webExtractPath% %filePath%
    echo �����ı��ļ�
    choice /t 1 /d y

    for /f "delims=" %%i in ('dir /b/a-d/s %transitionFolder%\*') do (
        call %binary2testPath% %%i -detailed -hexfloat
    )
    echo �����ɵ�ͬĿ¼%~nx1%_data��
    shift
    goto LOOP

:END
    echo Done!
pause