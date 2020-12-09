@echo off

:: Working directory
set BATCH_PATH=%~dp0
set BATCH_TO_PROJECT=..
set PROJECT_PATH=%BATCH_PATH%%BATCH_TO_PROJECT%

set BATCH=-batchmode
set QUIT=-quit
set PROJECT=-projectPath

:: Backup SrcPath
set BACKUP_ROOTPATH=-RootPath
set BACKUP_ROOTPATH_PARAMETER=Deployment

:: Backup AppDirectory
set BACKUP_APPDIRECTORY=-AppDirectory
set BACKUP_APPDIRECTORY_PARAMETER=0.0.4


echo %BATCH% %QUIT% -executeMethod Framework.AssetManagement.GameBuilder.Deployment.cmdDeploy  %BACKUP_ROOTPATH% %BACKUP_ROOTPATH_PARAMETER% %BACKUP_APPDIRECTORY% %BACKUP_APPDIRECTORY_PARAMETER%
"D:\Program Files\2020.2.0b5\Editor\Unity.exe" %BATCH% %QUIT% %PROJECT% %PROJECT_PATH% -executeMethod Framework.AssetManagement.GameBuilder.Deployment.cmdDeploy  %BACKUP_ROOTPATH% %BACKUP_ROOTPATH_PARAMETER% %BACKUP_APPDIRECTORY% %BACKUP_APPDIRECTORY_PARAMETER%

echo %errorlevel%

echo End Backup.....

pause