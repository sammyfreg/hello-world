// main.cpp
#include <stdio.h>
#include "ConditionalBuild.h"
#include "../ExampleLib/ExampleLib.h"

int main(int argc, char** argv)
{
	(void)argc;
	(void)argv;
    printf("Hello World\n");

	// Using a library include dependency
	ExampleLibCall();

	// This demontrate that certain source files can be excluded from build, based on Sharpmake build rules.
	// The same could easily be achieved using preprocessors defines but we wanted a simple sample of 
	// source file compiling selection using suffix.
	printf("Conditional Build Test\n");
	printf(" - Optimisation : %s\n", GetBuildName_Optim());
	printf(" - Plaftorm     : %s\n", GetBuildName_Platform());
	printf(" - Always Build : %s\n", GetBuildName_Unaffected());

	// Done
	printf("----------------------------------------\nPress 'Enter' to continue...\n");
	getchar();

	return 0;
}