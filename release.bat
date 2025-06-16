@echo off
setlocal enabledelayedexpansion

REM Récupère le dossier où se trouve ce script
set SCRIPT_DIR=%~dp0
set OUTPUT_DIR=%SCRIPT_DIR%publish
set TARGET_RUNTIMES=win-x64 win-arm64 linux-x64 linux-arm64 osx-x64 osx-arm64
set PROJECT_NAME=Il2CppAutoInterop.PostCompiler
set PROJECT_OUTPUT_NAME=Il2CppAutoInterop
set PROJECT_PATH=%SCRIPT_DIR%%PROJECT_NAME%\%PROJECT_NAME%.csproj

REM Nettoyage du dossier publish
if exist "%OUTPUT_DIR%" (
    rmdir /s /q "%OUTPUT_DIR%"
)
mkdir "%OUTPUT_DIR%"

REM Boucle sur les runtimes
for %%T in (%TARGET_RUNTIMES%) do (
    call :PublishTarget %%T
)

exit /b

:PublishTarget
setlocal

set "CURRENT_TARGET_RUNTIME=%~1"
set "CURRENT_OUTPUT_DIR=%OUTPUT_DIR%\%CURRENT_TARGET_RUNTIME%"

dotnet publish "%PROJECT_PATH%" ^
 --configuration Release ^
 --runtime %CURRENT_TARGET_RUNTIME% ^
 --output "%CURRENT_OUTPUT_DIR%" ^
 /p:PublishSingleFile=true ^
 /p:IncludeNativeLibrariesForSelfExtract=true ^
 /p:IncludeAllContentForSelfExtract=true ^
 /p:PublishTrimmed=false ^
 /p:SelfContained=true ^
 /p:DebugType=None ^
 /p:SolutionDir=%SCRIPT_DIR%

set "ZIP_FILE_NAME=%PROJECT_OUTPUT_NAME%.%CURRENT_TARGET_RUNTIME%.zip"
set "ZIP_FILE_PATH=%OUTPUT_DIR%\%ZIP_FILE_NAME%"

powershell -Command "Compress-Archive -Path '%CURRENT_OUTPUT_DIR%\*' -DestinationPath '%ZIP_FILE_PATH%'"
echo [+] Created archive: %ZIP_FILE_NAME%

rmdir /s /q "%CURRENT_OUTPUT_DIR%"

endlocal
goto :eof
