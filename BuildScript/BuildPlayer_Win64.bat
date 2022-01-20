:: 此文件基于ProjectPath/Build/路径工作，放置其他路径需要修改BATCH_TO_PROJECT

@echo off
echo "start building..."


:: Working directory
set BATCH_PATH=%~dp0
set BATCH_TO_PROJECT=..
set PROJECT_PATH=%BATCH_PATH%%BATCH_TO_PROJECT%
echo "	[PROJECT PATH]:"	%PROJECT_PATH%

set BUILD_TARGET=%BUILD_TARGET%
if "%BUILD_TARGET%" == "" (
	set BUILD_TARGET=Win64
)
set BUILD_PROFILE=Win64
echo "	[BUILD TARGET]:"	%BUILD_TARGET%
echo "	[BUILD PROFILE]:"	%BUILD_PROFILE%
if "%UNITY_PATH%" == "" (
	set UNITY_PATH="D:\Program Files\2021.2.3f1\Editor\Unity.exe"
)

:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
:: Bundle Output
set BUNDLE_PATH=%PROJECT_PATH%\Deployment\Latest\AssetBundles
echo "	[BUNDLE PATH]:"		%BUILD_PROFILE%

:: Player Output
if "%PLAYER_PATH%" == "" (
	set PLAYER_PATH=%PROJECT_PATH%\Deployment\Latest\Player
)
echo "	[PLAYER PATH]:"		%PLAYER_PATH%

:: Log
set LOG_PATH=%PROJECT_PATH%\Deployment\Latest\build_log.txt
echo "	[LOG PATH]:"		%LOG_PATH%

:: Build Mode: 0(Bundles & Player)、1(Bundles)、2(Player)
set BUILD_MODE_PARAMETER=0
echo "	[BUILD MODE]:"		%BUILD_MODE_PARAMETER%

if "%Development%" == "" (
	set Development=true
)
echo "	[Development]:"		%Development%

if "%useIL2CPP%" == "" (
	set useIL2CPP=false
)
echo "	[useIL2CPP]:"		%useIL2CPP%

if "%useMTRendering%" == "" (
	set useMTRendering=true
)
echo "	[useMTRendering]:"	%useMTRendering%

if "%RebuildBundles%" == "" (
	set RebuildBundles=false
)
echo "	[RebuildBundles]:"	%RebuildBundles%

set MACRODEFINES=%MACRODEFINES%
echo "	[MACRODEFINES]:"	%MACRODEFINES%

:: App Version(VersionNoChanged、VersionIncrease、VersionSpecific 1.2.3)

:: Fixed Command
set FIXED_COMMAND=-batchmode -quit -nographics -projectPath %PROJECT_PATH% -buildTarget %BUILD_TARGET% -executeMethod Framework.AssetManagement.GameBuilder.GameBuilder.cmdBuildGame -BuilderProfile %BUILD_PROFILE%
echo "	[FIXED COMMAND]:"	%FIXED_COMMAND%

:: Optional Command
set OVERRIDE_COMMAND=-bundlesOutput %BUNDLE_PATH% -playerOutput %PLAYER_PATH% -BuildMode %BUILD_MODE_PARAMETER% -VersionNoChanged -Development %Development% -useIL2CPP %useIL2CPP% -useMTRendering %useMTRendering% -RebuildBundles %RebuildBundles% -MacroDefines %MACRODEFINES%
echo "	[OVERRIDE COMMAND]:"	%OVERRIDE_COMMAND%

echo "	[Unity Path]:	" %UNITY_PATH%



"%UNITY_PATH%" %FIXED_COMMAND% %OVERRIDE_COMMAND%


echo %errorlevel%

rem if NOT %errorlevel% == 0 (
rem  exit %errorlevel%
rem )

echo "End building"

pause
