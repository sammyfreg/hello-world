@echo off
setlocal enabledelayedexpansion

:: ===========================================================================
:: 	Bash script to execute a 'make' command over all available configurations
::	of a solution. Windows needs a configured make, llvm, gcc environment.
::  Note: 	Solution can also be compiled by user, using the Visual Studio 
::			solution instead.
:: ---------------------------------------------------------------------------
:: [Batchfile Parameter 1] Solution name
:: [Batchfile Parameter 2] Command sent to Make
:: [Batchfile Parameter 3] (1) No keypress wait (other) Wait for keypress
:: ===========================================================================

:: ========================================================
::  INITIALISATION
:: ========================================================
:: List of configurations to build
set CONFIGS=(debugdefault, releasedefault, retaildefault, debugllvm, releasellvm, retailllvm)
set PLATFORM=(win32, win64)

set PROJECTPATH=%~dp0\..\..\_Projects

set /A CountSuccess = 0
set /A CountFailed 	= 0

IF not exist %PROJECTPATH% ( 
	echo ------------------------------------------------------------
	echo  Error
	echo ------------------------------------------------------------
	echo Please first generate solution/projects
	echo using Generate.bat
	goto last
)

pushd %PROJECTPATH%

:: ========================================================
::  Compile each available version
:: ========================================================
for %%s in %PLATFORM% do (
	set SLN_FILENAME=.\make_%1_%%s.make
	if exist !SLN_FILENAME! ( 
		for %%v in %CONFIGS% do (
			echo ------------------------------------------------------------
			echo   Config: %1 - %%s - %%v 
			echo ------------------------------------------------------------
			echo make -f"!SLN_FILENAME!" config=%%v %2
			make -f"!SLN_FILENAME!" config=%%v %2
			IF !ERRORLEVEL! EQU 0 (
				echo ====[ SUCCESS ]====
				set /A CountSuccess = !CountSuccess! + 1
			) ELSE (
				echo ====[ FAILED ]====
				set /A CountFailed = !CountFailed! + 1
			)
			echo.
		)
	)
)

echo ------------------------------------------------------------
echo  RESULTS: %1
echo ------------------------------------------------------------
ECHO   -Success: %CountSuccess%
ECHO   -Failed : %CountFailed%
popd

:last
IF "%3" == "1" GOTO end
pause

:end