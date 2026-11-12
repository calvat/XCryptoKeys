@echo off
rem Synchronise le clone avec GitHub (pull --rebase puis push) sans croiser un passage du bot. Voir sync.ps1.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0sync.ps1" %*
