@echo off 
"D:\program files\2019.3.3f1\Editor\Unity.exe" -projectPath "G:\MyGitHub\ResourcesManager" -quit -batchmode -buildTarget Android -executeMethod AssetManagement.GameBuilder.GameBuilder.cmdBuildGame -BuilderProfier android -UseAPKExpansionFiles -logFile "G:\MyGitHub\ResourcesManager\Deployment\build_android_log.txt"


echo %errorlevel%

rem if NOT %errorlevel% == 0 (
rem  exit %errorlevel%
rem )

pause