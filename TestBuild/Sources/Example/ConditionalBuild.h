#pragma once

// Demonstration of disabling compilation of certain source files, using suffix rules in Sharpmake
const char* GetBuildName_Optim();
const char* GetBuildName_Platform();
const char* GetBuildName_Unaffected();