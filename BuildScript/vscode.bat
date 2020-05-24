set BATCH_PATH=%~dp0
set WORKSPACE_FILENAME=..\PoolManager.code-workspace
set WORKSPACE_PATH=%BATCH_PATH%%WORKSPACE_FILENAME%

"C:\Users\Administrator\AppData\Local\Programs\Microsoft VS Code\Code.exe" %WORKSPACE_PATH% %1
