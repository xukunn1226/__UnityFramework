::@echo off

::generator c# code
rem step1. locate working directory
cd %~dp0
rem step2. delete all scripts
del ..\*.cs
cd .\Proto
rem step3. compile cs
::protoc -I=源地址     --csharp_out=目标地址      源地址/xxx.proto
for %%i in (".\*.proto") do protoc -I=.\ --csharp_out=..\..\ %%i




goto comment1
::设置自身的临时环境变量
setlocal
path=f:\
echo local environment variable
set path
endlocal
echo system environment variable.
set path
:comment1


goto comment2
::创建、设置、查看和删除环境变量
set xxx="c:\"
echo display all the variables begin with character c
set xxx
dir /w %xxx%
rem delete the xxx variable
set xxx=
set
:comment2



echo success
pause


