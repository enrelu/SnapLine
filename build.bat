@echo off
setlocal
set MSBUILD_PATH=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
if not exist "%MSBUILD_PATH%" set MSBUILD_PATH=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe

echo [INFO] Compilando proyecto con MSBuild de Windows...
"%MSBUILD_PATH%" "%~dp0Portafotos.csproj" /p:Configuration=Release /v:minimal
if %ERRORLEVEL% EQU 0 (
    echo.
    echo [OK] Compilacion completada con exito.
    echo [OK] Ejecutable generado en: bin\Release\Portafotos.exe
) else (
    echo.
    echo [ERROR] Error durante la compilacion.
)
pause
