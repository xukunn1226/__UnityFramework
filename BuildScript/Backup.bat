@echo off

:: Working directory
set BATCH_PATH=%~dp0
set BATCH_TO_PROJECT=..
set PROJECT_PATH=%BATCH_PATH%%BATCH_TO_PROJECT%

set BATCH=-batchmode
set QUIT=-quit
set PROJECT=-projectPath

:: Backup SrcPath
set BACKUP_SRCPATH=-SrcPath
set BACKUP_SRCPATH_PARAMETER=Deployment\Latest

:: Backup DstPath
set BACKUP_DSTPATH=-DstPath
set BACKUP_DSTPATH_PARAMETER=Deployment\Backup

:: Backup AppDirectory
set BACKUP_APPDIRECTORY=-AppDirectory
set BACKUP_APPDIRECTORY_PARAMETER=0.12.3


echo %BATCH% %QUIT% -executeMethod Framework.AssetManagement.Deployment.Deployment.cmdBackup  %BACKUP_SRCPATH% %BACKUP_SRCPATH_PARAMETER% %BACKUP_DSTPATH% %BACKUP_DSTPATH_PARAMETER% %BACKUP_APPDIRECTORY% %BACKUP_APPDIRECTORY_PARAMETER%
"D:\Program Files\2020.2.0b5\Editor\Unity.exe" %BATCH% %QUIT% %PROJECT% %PROJECT_PATH% -executeMethod Framework.AssetManagement.Deployment.Deployment.cmdBackup  %BACKUP_SRCPATH% %BACKUP_SRCPATH_PARAMETER% %BACKUP_DSTPATH% %BACKUP_DSTPATH_PARAMETER% 

:: %BACKUP_APPDIRECTORY% %BACKUP_APPDIRECTORY_PARAMETER%


echo End Backup.....

pause