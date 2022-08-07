@echo off

REM Launch Solution Generate
REM Use "GenerateProjects.bat 1" to skip the key press waiting (usefull for CI)

pushd %~dp0
Build\Sharpmake\Sharpmake.Application.exe /sources(@'Build\main.sharpmake.cs') /verbose 
popd

IF "%1" == "1" GOTO end
pause

:end