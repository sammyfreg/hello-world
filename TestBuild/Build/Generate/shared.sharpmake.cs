﻿using Sharpmake;
using System;
using System.IO; // Path
using System.Collections.Generic; //List

//=========================================================================
// Which toolchain should be used to build the targets
//=========================================================================
[Fragment, Flags]
public enum BuildToolset
{
	Default = 1 << 1,
	LLVM 	= 1 << 2
}

//=========================================================================
// Project Generation Settings
//=========================================================================
public class Settings
{	
	// Supported Target configurations by this build setup
	public const DevEnv 			SupportDevEnv 	= DevEnv.vs2019 | DevEnv.vs2022 | DevEnv.make;				// Programming Environment supported
	public const DevEnv				SupportLinux	= DevEnv.make | DevEnv.vs2022;								// Which Environments also support linux dev (Note: VS has some compiling path issues)
	public const Platform 			SupportPlatform = Platform.win32 | Platform.win64 | Platform.linux;
	public const BuildToolset 		SupportToolset	= BuildToolset.Default | BuildToolset.LLVM;
	public const Optimization		SupportOptim	= Optimization.Debug | Optimization.Release | Optimization.Retail;
	public static readonly string 	RootPath		= Path.Combine(Util.PathMakeStandard(AppDomain.CurrentDomain.BaseDirectory), "..", "..", "..");	// Root path of project from which everything is generated
}

//=============================================================================================
// Build Target
//=============================================================================================
public class CustomTarget : Target
{
	public CustomTarget() { }

	public CustomTarget(DevEnv inDevEnv, Platform inPlatform, Optimization inOptim, BuildToolset inToolset)
	: base(inPlatform, inDevEnv, inOptim, OutputType.Lib, Blob.NoBlob, BuildSystem.MSBuild)
	{		
		BuildToolset = inToolset;
	}
	
	public string GetConfigName()
	{
		return Optimization + BuildToolset.ToString();
	}
	
	public bool IsPlatformNameNeeded()
	{
		// Visual Studio allows win32/win64 in name project/config
		// Otherwise, needs to generate a distinct solution/project
		return (DevEnv.IsVisualStudio() && (Platform == Platform.win32 || Platform == Platform.win64)) == false;
	}
	
	//=========================================================================
	// Generates Targets for each Build System requested (if valid)
	// Note:	By default, generate all supported configurations 
	//=========================================================================
	static public CustomTarget[] CreateTargets( DevEnv inDevEnv 		= Settings.SupportDevEnv, 
												Platform inPlatform 	= Settings.SupportPlatform,
												Optimization inOptim	= Settings.SupportOptim,
												BuildToolset inToolset	= Settings.SupportToolset )
	{		
		List<CustomTarget> targets 	= new List<CustomTarget>();
		DevEnv[] envValues 			= (DevEnv[])Enum.GetValues(typeof(DevEnv));
		
		//---------------------------------------------------------------------
		// Remove all unsupported configuration by this project generator
		inDevEnv 	&= Settings.SupportDevEnv;
		inPlatform 	&= Settings.SupportPlatform;
		inOptim		&= Settings.SupportOptim;
		inToolset	&= Settings.SupportToolset;
												
		//---------------------------------------------------------------------
		// Visual Studio Support
		foreach (var devEnv in envValues)
		{
			bool isPow2 = (devEnv & (devEnv - 1)) == 0; // Reject composite enum values
			if (isPow2 && devEnv.IsVisualStudio() && (devEnv & inDevEnv) != 0 )
			{
				Platform platform	= inPlatform & (Platform.win32|Platform.win64);
				string VSPath		= devEnv.GetVisualStudioDir();
				if ( (platform != 0) && Util.DirectoryExists(VSPath) ){
					BuildToolset toolset = inToolset & BuildToolset.Default;
					
					/// Add Visual Studio LLVM support
					if(Util.FileExists(Path.Combine(ClangForWindows.Settings.LLVMInstallDirVsEmbedded(devEnv), "bin", "clang.exe" ))){
						toolset |= (inToolset & BuildToolset.LLVM);
					}
					targets.Add(new CustomTarget(devEnv, platform, inOptim, toolset));
				}
			}
		}

		//---------------------------------------------------------------------
		// Visual Studio Linux Support
		foreach (var devEnv in envValues) 
		{
			bool isPow2 = (devEnv & (devEnv - 1)) == 0; // Reject composite enum values
			if (isPow2 && devEnv.IsVisualStudio() && (devEnv & Settings.SupportLinux) != 0 && (devEnv & inDevEnv) != 0)
			{
				Platform platform 	= inPlatform & Platform.linux;
				string VSLinuxPath 	= Path.Combine(devEnv.GetVisualStudioDir(), "Common7", "IDE", "VC", "Linux");
				if ((platform != 0) && Util.DirectoryExists(VSLinuxPath)) {
					BuildToolset toolset = inToolset & BuildToolset.Default;
					// Add Visual Studio LLVM support
					// ...
					targets.Add(new CustomTarget(devEnv, platform, inOptim, toolset));
				}
			}
		}

		//---------------------------------------------------------------------
		// Makefile support
		if ((inDevEnv & DevEnv.make) != 0)
		{
			Platform platform 		= inPlatform & (Platform.win32 | Platform.win64);
			BuildToolset toolset 	= inToolset & (BuildToolset.Default | BuildToolset.LLVM);
			if ( (inPlatform & Platform.linux) != 0 && (Settings.SupportLinux & DevEnv.make) != 0) {
				platform |= Platform.linux;
			}

			targets.Add(new CustomTarget(DevEnv.make, platform, inOptim, toolset));
		}
		return targets.ToArray();
	}
	
	public BuildToolset BuildToolset;
}

//=============================================================================================
// PROJECT
//=============================================================================================
public class ProjectBase : Project
{
    public ProjectBase(string inName, bool inAddDefaultTarget, bool inIsExe)
	: base(typeof(CustomTarget))
    {
        Name 					= inName;
		IsExe 					= inIsExe;
		IsFileNameToLower		= false;
		IsTargetFileNameToLower = false;
		if( inAddDefaultTarget ){
			AddTargets(CustomTarget.CreateTargets());
		}
    }

	[Configure]
	public virtual void ConfigureAll(Project.Configuration conf, CustomTarget target)
	{
		//---------------------------------------------------------------------
		// Generic Projects Options
		conf.Name				= target.GetConfigName();
		conf.Output				= IsExe ? Project.Configuration.OutputType.Exe : Project.Configuration.OutputType.Lib;
		conf.ProjectFileName	= @"[project.Name]";
		conf.ProjectPath		= @"[solution.Name]";
		conf.ProjectPath 		= target.IsPlatformNameNeeded()	? Path.Combine(Settings.RootPath, "_Projects", "[target.DevEnv]_[target.Platform]" , "[project.Name]")
																: Path.Combine(Settings.RootPath, "_Projects", "[target.DevEnv]" , "[project.Name]");
	
		conf.IntermediatePath	= Path.Combine("[conf.ProjectPath]" , "obj" , "[target.Platform]_[conf.Name]");
		conf.TargetLibraryPath	= Path.Combine("[conf.ProjectPath]" , "lib" , "[target.Platform]_[conf.Name]");
		conf.TargetPath			= Path.Combine(Settings.RootPath, "_bin" , "[target.DevEnv]_[target.BuildToolset]_[target.Platform]");
		conf.TargetFileSuffix	= @"_[target.Optimization]";
		
		//---------------------------------------------------------------------
		// Visual Studios Options
		if( target.DevEnv.IsVisualStudio() ){
			if (target.Optimization == Optimization.Debug){
				conf.Options.Add(Options.Vc.Compiler.RuntimeLibrary.MultiThreadedDebugDLL);
				// Note: Once Clang debug library link error is fixed (in new clang release),
				// try removing 'MultiThreadedDebugDLL' and enabling asan for clang too
				if( target.DevEnv > DevEnv.vs2017 && target.BuildToolset == BuildToolset.Default ){
					conf.Options.Add(Options.Vc.Compiler.EnableAsan.Enable);
				}
			}
			else{
				conf.Options.Add(Options.Vc.Compiler.RuntimeLibrary.MultiThreadedDLL);
			}
		
			// Toolset Options
			if( target.BuildToolset == BuildToolset.Default ){
				conf.Defines.Add("_HAS_EXCEPTIONS=0"); 					// Prevents error in VisualStudio c++ library with NoExcept, like xlocale
			}
			else if ( target.BuildToolset == BuildToolset.LLVM ){
				conf.Options.Add(Options.Vc.General.PlatformToolset.ClangCL);
			}			
		}
		conf.Options.Add(Options.Vc.General.WindowsTargetPlatformVersion.Latest);
		conf.Options.Add(Options.Vc.General.CharacterSet.Unicode);
		conf.Options.Add(Options.Vc.General.TreatWarningsAsErrors.Enable);
		conf.Options.Add(Options.Vc.Linker.TreatLinkerWarningAsErrors.Enable);
		conf.Options.Add(Linux.Options.General.VcPlatformToolset.WSL2_1_0);
		
		//---------------------------------------------------------------------
		// Makefile Options
		if( target.DevEnv == DevEnv.make ){
			
			// Toolset Options
			if ( target.BuildToolset == BuildToolset.LLVM ){
				conf.Options.Add(Options.Makefile.General.PlatformToolset.Clang);
			}
			//TODO: This is a workaround for issue with 'FixupLibraryNames' on makefiles
			if( IsExe == false && (target.Platform == Platform.win32 || target.Platform == Platform.win64)){
				conf.TargetFileFullExtension = ".a";
			}
		}
		
		//---------------------------------------------------------------------									
		// LLVM Options
		
		//---------------------------------------------------------------------									
		// SF
		//conf.IsFastBuild = true;
		//conf.FastBuildBlobbed = target.Blob == Blob.FastBuildUnitys;

		// Force writing to pdb from different cl.exe process to go through the pdb server
		//conf.AdditionalCompilerOptions.Add("/FS");
	}
	
	bool IsExe;
}

public class ProjectBaseExe : ProjectBase
{
    public ProjectBaseExe(string inName, bool inAddDefaultTarget)
	: base(inName, inAddDefaultTarget, true)
	{
	}
}

public class ProjectBaseLib : ProjectBase
{
    public ProjectBaseLib(string inName, bool inAddDefaultTarget)
	: base(inName, inAddDefaultTarget, false)
	{
	}
}

//=============================================================================================
// SOLUTIONS
//=============================================================================================
public class SolutionBase : Sharpmake.Solution
{
	public SolutionBase(string inName, bool inAddDefaultTarget)
	: base(typeof(CustomTarget))
	{
		Name					= inName;
		IsFileNameToLower		= false;
		if( inAddDefaultTarget ){
			AddTargets(CustomTarget.CreateTargets());
		}
	}

	[Configure()]
	public virtual void ConfigureAll(Configuration conf, CustomTarget target)
	{
		conf.Name				= target.GetConfigName();
		conf.SolutionFileName	= "[target.DevEnv]_[solution.Name]" + (target.IsPlatformNameNeeded() ? "_[target.Platform]" : "");
		conf.SolutionPath 		= Path.Combine(Settings.RootPath, "_Projects");
	}
}