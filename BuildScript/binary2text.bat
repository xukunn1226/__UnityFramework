@echo off
:LOOP
    :: 使用方式：将脚本直接放到C:\Users\lin\AppData\Roaming\Microsoft\Windows\SendTo目录下，后续即可通过右键AB，点击“发送到” 选择对应的脚本
    rem 前提是将下方的webExtractPath和binary2testPath变量的unity路径替换成你本地unity安装目录

    set webExtractPath="D:\Program Files\2019.3.3f1\Editor\Data\Tools\WebExtract.exe"
    set binary2testPath="D:\Program Files\2019.3.3f1\Editor\Data\Tools\binary2text.exe"
    set /p filePath="请输入Bundle Path: "
    set transitionFolder=%filePath%_data
	
    if not exist %webExtractPath% (
        echo 不存在%webExtractPath%
        goto END
    )	
	
    if not exist %binary2testPath% (
        echo 不存在%binary2testPath%
        goto END
    )
	
    if %filePath%! == ! (
        goto END
    )
    call %webExtractPath% %filePath%
    echo 生成文本文件
    choice /t 1 /d y

    for /f "delims=" %%i in ('dir /b/a-d/s %transitionFolder%\*') do (
        call %binary2testPath% %%i -detailed -hexfloat
    )
    echo 已生成到同目录%~nx1%_data下
    shift
    goto LOOP

:END
    echo Done!
pause