@echo off
REM =============================================================================
REM  Remove all generated folders
REM =============================================================================
FOR /d %%d IN ("_*") DO @IF EXIST "%%d" rd /s /q "%%d"