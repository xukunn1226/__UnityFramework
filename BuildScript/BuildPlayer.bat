:: 此文件基于ProjectPath/Build/路径工作，放置其他路径需要修改BATCH_TO_PROJECT

@echo off
echo Starting Build Process....


:: Working directory
set BATCH_PATH=%~dp0
set BATCH_TO_PROJECT=..
set PROJECT_PATH=%BATCH_PATH%%BATCH_TO_PROJECT%

set PLATFORM_NAME=%~1
set PROFILE_NAME=%~2


:: Build profile
set BUILD_PROFILE=-BuilderProfile

:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Bundle Output
set BUNDLE_OUTPUT=-bundlesOutput
set BUNDLE_PATH=%PROJECT_PATH%\Deployment\Latest\AssetBundles

:: Player Output
set PLAYER_OUTPUT=-playerOutput
set PLAYER_PATH=%PROJECT_PATH%\Deployment\Latest\Player

:: Log
set LOG_FILE=-logFile
set LOG_PATH=%PROJECT_PATH%\Deployment\Latest\build_log.txt

:: Build Mode: 0(Bundles & Player)、1(Bundles)、2(Player)
set BUILD_MODE=-BuildMode
set BUILD_MODE_PARAMETER=0

:: App Version(VersionNoChanged、VersionIncrease、VersionSpecific 1.2.3)
set APP_VERSION=-VersionNoChanged

:: Fixed Command
set FIXED_COMMAND=-batchmode -quit -nographics -projectPath %PROJECT_PATH% -buildTarget %PLATFORM_NAME% -executeMethod Framework.AssetManagement.GameBuilder.GameBuilder.cmdBuildGame -BuilderProfile %PROFILE_NAME%

:: Optional Command
set OPTIONAL_COMMAND=%BUNDLE_OUTPUT% %BUNDLE_PATH% %PLAYER_OUTPUT% %PLAYER_PATH% %LOG_FILE% %LOG_PATH% %BUILD_MODE% %BUILD_MODE_PARAMETER% %APP_VERSION%

echo "D:\Program Files\2021.2.3f1\Editor\Unity.exe" %FIXED_COMMAND% %OPTIONAL_COMMAND%
"D:\Program Files\2021.2.3f1\Editor\Unity.exe" %FIXED_COMMAND% %OPTIONAL_COMMAND%


echo %errorlevel%

rem if NOT %errorlevel% == 0 (
rem  exit %errorlevel%
rem )

echo End Build Process.....

pause