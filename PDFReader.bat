@echo off
setlocal

cd /d "%~dp0"
dotnet run --project "%~dp0PDFReader\PDFReader.csproj" --configuration Release
set "exitCode=%ERRORLEVEL%"

if not "%exitCode%"=="0" (
    echo.
    echo PDFReader exited with code %exitCode%.
)

pause
exit /b %exitCode%
