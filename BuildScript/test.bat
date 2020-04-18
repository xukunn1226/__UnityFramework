::@echo off

Setlocal enabledelayedexpansion

set /p var1="请输入一个字符串："
set /p var2="请输入一个字符串："


echo errorlevel is %ERRORLEVEL%

if NOT %ERRORLEVEL% == 0
(
	%var1%
)
else
(
	%var2%
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