@echo off
powershell.exe -ExecutionPolicy Unrestricted %~dp0\detect.ps1 %1

REM exit /b 1