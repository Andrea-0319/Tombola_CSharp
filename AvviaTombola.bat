@echo off
setlocal

cd /d "%~dp0"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo .NET SDK non trovato nel PATH.
    echo Installa .NET e riprova.
    pause
    exit /b 1
)

echo Avvio Tombola...
dotnet run --project "%~dp0Tombola.csproj"
set "exitCode=%errorlevel%"

if /I "%~1"=="--no-pause" (
    endlocal & exit /b %exitCode%
)

echo.
echo Gioco terminato con codice: %exitCode%
pause
endlocal & exit /b %exitCode%
