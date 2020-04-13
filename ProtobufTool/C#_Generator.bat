@echo off

::step 1: generator c# code
del .\Src\*.cs
cd .\Proto
for %%i in (".\*.proto") do protoc -I=.\ --csharp_out=..\Src %%i

::step 2: copy generated code to workspace
cd ..\
del "..\Assets\Scripts\Protocol\*.cs"
copy /y ".\Src\*.cs" "..\Assets\Scripts\Protocol\"


echo success
pause

::https://developers.google.com/protocol-buffers/docs/proto3
::https://developers.google.com/protocol-buffers/docs/reference/csharp-generated