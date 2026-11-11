@echo off
rem Lance install.ps1 sans changer la politique d'ex??cution de Windows. Les options sont transmises (ex. install.cmd -WhatIf).
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1" %*
