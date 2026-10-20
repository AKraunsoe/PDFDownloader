@echo off
setlocal

cd /d "%~dp0"
dotnet build "%~dp0PDFReader\PDFReader.csproj" --configuration Release -consoleloggerparameters:ErrorsOnly >nul 2>&1
set "exitCode=%ERRORLEVEL%"

if not "%exitCode%"=="0" goto :finish

dotnet run --project "%~dp0PDFReader\PDFReader.csproj" --configuration Release --no-build
set "exitCode=%ERRORLEVEL%"

:finish
if not "%exitCode%"=="0" (
    echo.
    echo PDFReader exited with code %exitCode%.
)

pause
exit /b %exitCode%