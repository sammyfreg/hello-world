@echo off
REM =============================================================================
REM  Generate the Engine's Solutions and Projects needed to build it
REM  Use "Generate.bat 1" to skip the key press waiting 
REM  (useful for Continous Integration tests)
REM =============================================================================
pushd %~dp0
Build\Generate\Sharpmake\Sharpmake.Application.exe /sources(@'Build\Generate\main.sharpmake.cs') /verbose 
popd

IF "%1" == "1" GOTO end
pause

:end