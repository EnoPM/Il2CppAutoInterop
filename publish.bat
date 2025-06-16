@echo off
setlocal enabledelayedexpansion

set DOTNET_RUNTIME=C:\Users\EnoPM\.dotnet\dotnet.exe
set TARGET_RUNTIMES=win-x64 win-arm64 linux-x64 linux-arm64 osx-x64 osx-arm64
set OUTPUT_DIR=D:\GameDevelopment\Il2CppAutoInterop-publish
set PROJECT_NAME=Il2CppAutoInterop.PostCompiler
set PROJECT_OUTPUT_NAME=Il2CppAutoInterop
set PROJECT_PATH="%PROJECT_NAME%\%PROJECT_NAME%.csproj"

for %%T in (%TARGET_RUNTIMES%) do (
    call :PublishTarget %%T
)

exit /b

:PublishTarget
setlocal

set "CURRENT_TARGET_RUNTIME=%~1"
set "CURRENT_OUTPUT_DIR=%OUTPUT_DIR%\%CURRENT_TARGET_RUNTIME%"

%DOTNET_RUNTIME% publish "%PROJECT_PATH%" ^
 --configuration Release ^
 --runtime %CURRENT_TARGET_RUNTIME% ^
 --output "%CURRENT_OUTPUT_DIR%" ^
 /p:PublishSingleFile=true ^
 /p:IncludeNativeLibrariesForSelfExtract=true ^
 /p:IncludeAllContentForSelfExtract=true ^
 /p:PublishTrimmed=false ^
 /p:SelfContained=true ^
 /p:DebugType=None ^
 /p:SolutionDir=%CD%\

set "ZIP_FILE_NAME=%PROJECT_OUTPUT_NAME%.%CURRENT_TARGET_RUNTIME%.zip"
set "ZIP_FILE_PATH=%OUTPUT_DIR%\%ZIP_FILE_NAME%"

if exist "%ZIP_FILE_PATH%" (
    del "%ZIP_FILE_PATH%" >nul
    echo [-] Deleted existing archive: %ZIP_FILE_NAME%
)

powershell -Command "Compress-Archive -Path '%CURRENT_OUTPUT_DIR%\*' -DestinationPath '%OUTPUT_DIR%\%PROJECT_OUTPUT_NAME%.%CURRENT_TARGET_RUNTIME%.zip'"
echo [+] Created archive: %PROJECT_OUTPUT_NAME%.%CURRENT_TARGET_RUNTIME%.zip

rmdir /s /q "%CURRENT_OUTPUT_DIR%"
 
endlocal
goto :eof