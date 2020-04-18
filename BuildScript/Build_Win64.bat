:: 此文件基于ProjectPath/Build/路径工作，放置其他路径需要修改BATCH_TO_PROJECT

@echo off
echo Starting Build Process....


:: Working directory
set BATCH_PATH=%~dp0
set BATCH_TO_PROJECT=..
set PROJECT_PATH=%BATCH_PATH%%BATCH_TO_PROJECT%


:: Common options
set BATCH=-batchmode
set QUIT=-quit
set PROJECT=-projectPath
::echo Common options: %BATCH% %QUIT%
::echo Project path: %PROJECT_PATH%


:: Build target
set BUILD_TARGET=-buildTarget
set PLATFORM_NAME=Win64
::echo Build target: %BUILD_TARGET% %PLATFORM_NAME%


:: Build script
set BUILD_SCRIPT=-executeMethod Framework.GameBuilder.GameBuilder.cmdBuildGame
::echo Build script: %BUILD_SCRIPT%


:: Build profile
set BUILD_PROFILE=-BuilderProfile
set PROFILE_NAME=win64
::echo Build Profile: %BUILD_PROFILE% %PROFILE_NAME%


:: Bundle Output
set BUNDLE_OUTPUT=-bundlesOutput
set BUNDLE_PATH=%PROJECT_PATH%\Deployment\AssetBundles
::echo Bundle Output: %BUNDLE_OUTPUT% %BUNDLE_PATH%


:: Player Output
set PLAYER_OUTPUT=-playerOutput
set PLAYER_PATH=%PROJECT_PATH%\Deployment\Player
::echo Player Output: %PLAYER_OUTPUT% %PLAYER_PATH%


:: Log
set LOG_FILE=-logFile
set LOG_PATH=%PROJECT_PATH%\Deployment\build_win64_log.txt
::echo Log path: %LOG_FILE% %LOG_PATH%


:: Fixed Command
set FIXED_COMMAND=%BATCH% %QUIT% %PROJECT% %PROJECT_PATH% %BUILD_TARGET% %PLATFORM_NAME% %BUILD_SCRIPT% %BUILD_PROFILE% %PROFILE_NAME%
echo Fixed command: %FIXED_COMMAND%


:: Optional Command
set OPTIONAL_COMMAND=%BUNDLE_OUTPUT% %BUNDLE_PATH% %PLAYER_OUTPUT% %PLAYER_PATH% %LOG_FILE% %LOG_PATH%
echo Optional command: %OPTIONAL_COMMAND%

echo "D:\program files\2019.3.3f1\Editor\Unity.exe" %FIXED_COMMAND% %OPTIONAL_COMMAND%
"D:\program files\2019.3.3f1\Editor\Unity.exe" %FIXED_COMMAND% %OPTIONAL_COMMAND%


::"D:\program files\2019.3.3f1\Editor\Unity.exe" %BATCH% %QUIT% %PROJECT% %PROJECT_PATH% -buildTarget Win64 -executeMethod Framework.GameBuilder.GameBuilder.cmdBuildGame -BuilderProfile win64 -bundlesOutput ./Deployment/AssetBundles -logFile "G:\MyGitHub\PoolManager\Deployment\build_win64_log.txt"






echo %errorlevel%

rem if NOT %errorlevel% == 0 (
rem  exit %errorlevel%
rem )

echo End Build Process.....

pause