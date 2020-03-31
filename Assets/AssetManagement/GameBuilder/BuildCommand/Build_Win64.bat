@echo off 
"D:\program files\2019.3.3f1\Editor\Unity.exe" -projectPath "G:\MyGitHub\ResourcesManager" -quit -batchmode -buildTarget Win64 -executeMethod AssetManagement.GameBuilder.GameBuilder.cmdBuildGame -BuilderProfier win64 -bundlesOutput ./Deployment/AssetBundles -logFile "G:\MyGitHub\ResourcesManager\Deployment\build_win64_log.txt"


echo %errorlevel%

rem if NOT %errorlevel% == 0 (
rem  exit %errorlevel%
rem )

pause