// main.cpp
#include <stdio.h>
#include "../ExampleLib/ExampleLib.h"

int main(int argc, char** argv)
{
	(void)argc;
	(void)argv;
    printf("Hello World\n");
	ExampleLibCall();
	printf("----------------------------------------\nPress 'Enter' to continue...\n");
	getchar();

	return 0;
}