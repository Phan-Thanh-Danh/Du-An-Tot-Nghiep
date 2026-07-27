@echo off
echo Đang build ExamGuard.Agent cho nhieu nen tang (Windows, Linux, macOS)...
echo.

set PROJECT_PATH=ExamGuard.Agent\ExamGuard.Agent.csproj
set OUTPUT_DIR=publish

set BUILD_FLAGS=-c Release --self-contained -p:PublishSingleFile=true -p:DebugType=None -p:DebugSymbols=false -p:IsTransformWebConfigDisabled=true -p:PublishTrimmed=true -p:EnableCompressionInSingleFile=true

echo [1] Build cho Windows x64...
dotnet publish "%PROJECT_PATH%" -r win-x64 %BUILD_FLAGS% -o "%OUTPUT_DIR%\win-x64"

echo.
echo [2] Build cho Windows x86 (32-bit)...
dotnet publish "%PROJECT_PATH%" -r win-x86 %BUILD_FLAGS% -o "%OUTPUT_DIR%\win-x86"

echo.
echo [3] Build cho Windows ARM64 (Snapdragon/ARM)...
dotnet publish "%PROJECT_PATH%" -r win-arm64 %BUILD_FLAGS% -o "%OUTPUT_DIR%\win-arm64"

echo.
echo [4] Build cho Linux x64...
dotnet publish "%PROJECT_PATH%" -r linux-x64 %BUILD_FLAGS% -o "%OUTPUT_DIR%\linux-x64"

echo.
echo [5] Build cho Linux ARM64 (Raspberry Pi, AWS Graviton...)...
dotnet publish "%PROJECT_PATH%" -r linux-arm64 %BUILD_FLAGS% -o "%OUTPUT_DIR%\linux-arm64"

echo.
echo [6] Build cho macOS x64 (Chip Intel)...
dotnet publish "%PROJECT_PATH%" -r osx-x64 %BUILD_FLAGS% -o "%OUTPUT_DIR%\osx-x64"

echo.
echo [7] Build cho macOS ARM64 (Apple Silicon M1/M2/M3)...
dotnet publish "%PROJECT_PATH%" -r osx-arm64 %BUILD_FLAGS% -o "%OUTPUT_DIR%\osx-arm64"

echo.
echo Dang don dep cac file phu (pdb, web.config, staticwebassets...)...
del /s /q "%OUTPUT_DIR%\*.pdb" >nul 2>&1
del /s /q "%OUTPUT_DIR%\*.staticwebassets.endpoints.json" >nul 2>&1
del /s /q "%OUTPUT_DIR%\aspnetcorev2_inprocess.dll" >nul 2>&1
del /s /q "%OUTPUT_DIR%\web.config" >nul 2>&1
for /d /r "%OUTPUT_DIR%" %%d in (publish) do if exist "%%d" rmdir /s /q "%%d" >nul 2>&1

echo.
echo Dang tao file launcher cho Linux (ExamGuard.desktop va Chay-Agent.sh)...
echo [Desktop Entry]> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
echo Version=1.0>> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
echo Type=Application>> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
echo Name=ExamGuard Agent>> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
echo Comment=ExamGuard Agent cho Linux>> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
echo Exec=bash -c "cd \"$$(dirname \"%%k\")\" ^&^& chmod +x ./ExamGuard.Agent ^&^& ./ExamGuard.Agent">> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
echo Icon=utilities-terminal>> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
echo Terminal=true>> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
echo Categories=Utility;>> "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop"
copy /y "%OUTPUT_DIR%\linux-x64\ExamGuard.desktop" "%OUTPUT_DIR%\linux-arm64\ExamGuard.desktop" >nul

echo #!/bin/bash> "%OUTPUT_DIR%\linux-x64\Chay-Agent.sh"
echo cd "$(dirname "$0")">> "%OUTPUT_DIR%\linux-x64\Chay-Agent.sh"
echo chmod +x ./ExamGuard.Agent>> "%OUTPUT_DIR%\linux-x64\Chay-Agent.sh"
echo ./ExamGuard.Agent>> "%OUTPUT_DIR%\linux-x64\Chay-Agent.sh"
copy /y "%OUTPUT_DIR%\linux-x64\Chay-Agent.sh" "%OUTPUT_DIR%\linux-arm64\Chay-Agent.sh" >nul

echo.
echo Dang nen file .tar.gz cho Linux/macOS (Giu nguyen quyen chmod +x khi giai nen)...
tar -czvf "%OUTPUT_DIR%\ExamGuard-Agent-linux-x64.tar.gz" -C "%OUTPUT_DIR%\linux-x64" .
tar -czvf "%OUTPUT_DIR%\ExamGuard-Agent-linux-arm64.tar.gz" -C "%OUTPUT_DIR%\linux-arm64" .
tar -czvf "%OUTPUT_DIR%\ExamGuard-Agent-osx-x64.tar.gz" -C "%OUTPUT_DIR%\osx-x64" ExamGuard.Agent
tar -czvf "%OUTPUT_DIR%\ExamGuard-Agent-osx-arm64.tar.gz" -C "%OUTPUT_DIR%\osx-arm64" ExamGuard.Agent

echo.
echo Hoan tat! 
echo File nén cho Linux/macOS (.tar.gz) da duoc tao tai thu muc publish/

