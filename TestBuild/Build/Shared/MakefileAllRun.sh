#!/bin/bash

# ===========================================================================
# 	Batfile to execute a 'make' command over all available configurations
#	of a solution. Linux needs a configured make, llvm, gcc environment.
#  Note: 	Solution can also be compiled by user, using the Visual Studio 
#			solution instead.
# ---------------------------------------------------------------------------
# [Batchfile Parameter 1] Solution name
# [Batchfile Parameter 2] Command sent to Make
# ===========================================================================

# List of configurations to build
CONFIGS="debugdefault releasedefault retaildefault debugllvm releasellvm retailllvm"

SCRIPT_RELATIVE_DIR=$(dirname "${BASH_SOURCE[0]}")
SCRIPT_RELATIVE_DIR+="/../../_Projects"
CountSuccess=0
CountFailed=0

pushd ${SCRIPT_RELATIVE_DIR} >> /dev/null

for val in $CONFIGS; do
	echo ----------------------------------------
	echo   Config: $val
	echo ----------------------------------------
    make -f make_${1}_linux.make config=$val $2
	if [ $? -eq 0 ] ; then
		echo " ====[ SUCCESS ]===="
		CountSuccess=$((CountSuccess+1))
	else
		echo " ====[ FAILED ]===="
		CountFailed=$((CountFailed+1))
	fi
	echo " "
done

echo ----------------------------------------
echo  RESULTS
echo ----------------------------------------
echo   -Success: $CountSuccess
echo   -Failed : $CountFailed

popd >> /dev/null
