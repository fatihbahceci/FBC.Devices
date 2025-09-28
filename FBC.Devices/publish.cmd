@echo off
set CONFIG=Release
set TF=net8.0
REM dotnet restore --runtime linux-x64
REM dotnet restore --runtime win-x64

REM --- Windows x64 self-contained publish ---
echo Publishing Windows x64 SelfContained...
dotnet publish ^
  --configuration %CONFIG% ^
  --framework %TF% ^
  --runtime win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:PublishDir=bin\Release\win-x64_SelfContained\

if %ERRORLEVEL% neq 0 (
    echo Windows publish failed.
	pause
    exit /b %ERRORLEVEL%
)

REM --- Linux x64 self-contained publish ---
echo Publishing Linux x64 SelfContained...
dotnet publish ^
  --configuration %CONFIG% ^
  --framework %TF% ^
  --runtime linux-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:PublishDir=bin\Release\linux-x64_SelfContained\

if %ERRORLEVEL% neq 0 (
    echo Linux publish failed.
	pause
    exit /b %ERRORLEVEL%
)

echo All publish operations completed successfully.
pause
