#!/bin/bash
SCRIPT_FILE=$(dirname "${BASH_SOURCE[0]}")
SCRIPT_FILE+="/Shared/MakefileAllRun.sh"

$SCRIPT_FILE "SolutionExample" "all"
