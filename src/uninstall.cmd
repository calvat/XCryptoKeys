@echo off
rem Lance uninstall.ps1 sans changer la politique d'ex??cution de Windows (ex. uninstall.cmd -RemoveFiles).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0uninstall.ps1" %*
