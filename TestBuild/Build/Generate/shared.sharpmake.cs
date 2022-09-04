using Sharpmake;
using System;
using System.IO; // Path
using System.Collections.Generic; //List

//=========================================================================
// Which toolchain should be used to build the targets
//=========================================================================
[Fragment, Flags]
public enum BuildToolset
{
	Default 	= 1 << 0,
	LLVM 		= 1 << 1
}

[Fragment, Flags]
public enum GraphicsApi
{
	None 		= 1 << 0,
	DirectX11 	= 1 << 1,
	DirectX12 	= 1 << 2,
	OpenGL4_6	= 1 << 3,
	Vulkan		= 1 << 4,
}

//=========================================================================
// Project Generation Settings
//=========================================================================
public class Settings
{	
	// Supported Target configurations by our Sharpmake files
	public const DevEnv 			SupportWindowDevEnv 		= DevEnv.vs2019 | DevEnv.vs2022 | DevEnv.make;
	public const Platform 			SupportWindowPlatform 		= Platform.win32 | Platform.win64;
	public const BuildToolset 		SupportWindowToolset		= BuildToolset.Default | BuildToolset.LLVM;
	public const Optimization		SupportWindowOptim			= Optimization.Debug | Optimization.Release | Optimization.Retail;		
	public const GraphicsApi		SupportWindowGraphicsApi	= GraphicsApi.None | GraphicsApi.DirectX11 | GraphicsApi.DirectX12 | GraphicsApi.OpenGL4_6 | GraphicsApi.Vulkan;
	
	public const DevEnv 			SupportLinuxDevEnv 			= DevEnv.vs2022 | DevEnv.make;				// (Note: VS has some compiling path issues)
	public const Platform 			SupportLinuxPlatform 		= Platform.linux;
	public const BuildToolset 		SupportLinuxToolset			= BuildToolset.Default | BuildToolset.LLVM;
	public const Optimization		SupportLinuxOptim			= Optimization.Debug | Optimization.Release | Optimization.Retail;		
	public const GraphicsApi		SupportLinuxGraphicsApi		= GraphicsApi.None | GraphicsApi.OpenGL4_6 | GraphicsApi.Vulkan;
	
	// Utility arrays listing all fragment values per enum
	public readonly static DevEnv[]			DevEnvArray 		= InitDevEnvArray();
	public readonly static Platform[]		PlatformArray		= InitPlatformArray();
	public readonly static BuildToolset[] 	ToolsetArray		= (BuildToolset[])Enum.GetValues(typeof(BuildToolset));
	public readonly static Optimization[] 	OptimArray			= (Optimization[])Enum.GetValues(typeof(Optimization));
	public readonly static GraphicsApi[] 	GraphicsApiArray	= (GraphicsApi[])Enum.GetValues(typeof(GraphicsApi));
	
	// Special case for DevEnv enum, that contains certain fragment combination values. 
	// We only want individual values
	static DevEnv[] InitDevEnvArray()
	{
		List<DevEnv> devEnvValueGood 	= new List<DevEnv>();
		DevEnv[] devEnvValueAll 		= (DevEnv[])Enum.GetValues(typeof(DevEnv));
		foreach (var devEnv in devEnvValueAll)
		{	
			// Once compositve value found, we know anything coming after is invalud
			bool compositeValueFound = (devEnv & (devEnv - 1)) != 0; 
			if ( compositeValueFound )
				break;
			devEnvValueGood.Add(devEnv);
		}
	
		return devEnvValueGood.ToArray();
	}

	// Special case for Platform enum that contains invalid values
	static Platform[] InitPlatformArray()
	{
		List<Platform> platformValueGood	= new List<Platform>();
		Platform[] platformValueAll			= (Platform[])Enum.GetValues(typeof(Platform));
		
		foreach (var platform in platformValueAll)
		{
			if (platform != (Platform)(-1))
				platformValueGood.Add(platform);
		}

		return platformValueGood.ToArray();
	}
}

public class TargetSettings
{
	// Values used by project generator (can be customized by user)
	public DevEnv 			WantedWindowDevEnv 		= Settings.SupportWindowDevEnv;
	public Platform 		WantedWindowPlatform 	= Settings.SupportWindowPlatform;
	public BuildToolset 	WantedWindowToolset		= Settings.SupportWindowToolset;
	public Optimization		WantedWindowOptim		= Settings.SupportWindowOptim;
	public GraphicsApi		WantedWindowGraphicsApi	= GraphicsApi.None;
		   
	public DevEnv 			WantedLinuxDevEnv 		= Settings.SupportLinuxDevEnv;
	public Platform 		WantedLinuxPlatform 	= Settings.SupportLinuxPlatform;
	public BuildToolset 	WantedLinuxToolset		= Settings.SupportLinuxToolset;
	public Optimization		WantedLinuxOptim		= Settings.SupportLinuxOptim;
	public GraphicsApi		WantedLinuxGraphicsApi	= GraphicsApi.None;

	public string			RootPath				= Path.Combine(Environment.CurrentDirectory);  // Root path of project from which everything is generated
	
	public bool 			IsExe					= true;											// Set in Project constructor
	
	//---------------------------------------------------------------------------------------------
	// Create all wanted Targets by this settings
	// Note: Can override and customize this
	//---------------------------------------------------------------------------------------------
	public virtual CustomTarget[] Create()
	{
		List<CustomTarget> targets = new List<CustomTarget>();
		targets.AddRange(CreateVisualStudioWindow());
		targets.AddRange(CreateVisualStudioLinux());
		targets.AddRange(CreateMakefileWindow());
		targets.AddRange(CreateMakefileLinux());
		return targets.ToArray();
	}
	
	//---------------------------------------------------------------------------------------------
	// Receive 1 value per fragment mask, and must return true when this si a valid combination
	// Usefull method override to add custom rules disabling certain target configuration
	//---------------------------------------------------------------------------------------------
	public virtual bool IsValidTarget(DevEnv inDevEnv, Platform inPlatform, BuildToolset inToolset, Optimization inOptim, GraphicsApi inGraphicsApi)
	{
		// Temp removal of 'Windows Makefile + LLVM' because of a library path issue
		if( (inDevEnv & DevEnv.make) != 0 && (inPlatform & Settings.SupportWindowPlatform) != 0 ){
			inToolset &= ~BuildToolset.LLVM;
		}
		
		return ( (inDevEnv != 0) && (inPlatform != 0) && (inToolset != 0) && (inOptim != 0) && (inGraphicsApi != 0) );
	}
	
	//---------------------------------------------------------------------------------------------
	// Test each target combination one by one, adding them if considered valid
	//---------------------------------------------------------------------------------------------
	public void AddTarget(ref List<CustomTarget> outTargets, DevEnv inDevEnv, Platform inPlatform, BuildToolset inToolset, Optimization inOptim, GraphicsApi inGraphicsApi)
	{	
		foreach (var devEnv in Settings.DevEnvArray)
		{
			if( (devEnv & inDevEnv) == 0) continue;
			foreach (var platform in Settings.PlatformArray)
			{
				if( (platform & inPlatform) == 0) continue;
				foreach (var toolset in Settings.ToolsetArray)
				{
					if( (toolset & inToolset) == 0) continue;
					foreach (var optim in Settings.OptimArray)
					{
						if( (optim & inOptim) == 0) continue;
						foreach (var gfxApi in Settings.GraphicsApiArray)
						{
							if( (gfxApi & inGraphicsApi) == 0) continue;
							if( IsValidTarget(devEnv, platform, toolset, optim, gfxApi) ){
								outTargets.Add(new CustomTarget(devEnv, platform, optim, toolset, gfxApi));
							}
						}	
					}
				}
			}	
		}
	}
	
	//---------------------------------------------------------------------------------------------
	// Visual Studio Windows Support
	//---------------------------------------------------------------------------------------------
	public virtual List<CustomTarget> CreateVisualStudioWindow()
	{
		List<CustomTarget> targets = new List<CustomTarget>();
		
		// Remove all unsupported configuration by this project generator
		DevEnv wantDevEnv 			= (WantedWindowDevEnv & Settings.SupportWindowDevEnv);
		Platform wantPlatform 		= (WantedWindowPlatform & Settings.SupportWindowPlatform);
		BuildToolset wantToolset	= (WantedWindowToolset & Settings.SupportWindowToolset);
		Optimization wantOptim		= (WantedWindowOptim & Settings.SupportWindowOptim);
		GraphicsApi wantGraphicsApi	= (WantedWindowGraphicsApi & Settings.SupportWindowGraphicsApi);

		// Early out when no supported target detected
		if( wantDevEnv == 0 || wantPlatform == 0 || wantToolset == 0 || wantOptim == 0 || wantGraphicsApi == 0 )
			return targets;
		
		// Generate requested Targets
		foreach (var devEnv in Settings.DevEnvArray)
		{
			if ( devEnv.IsVisualStudio() && (devEnv & wantDevEnv) != 0 )
			{
				string VSPath = devEnv.GetVisualStudioDir();
				if ( Util.DirectoryExists(VSPath) ){					
					/// Remove Visual Studio LLVM target support if not installed
					bool foundLLVM 			= Util.FileExists(Path.Combine(ClangForWindows.Settings.LLVMInstallDirVsEmbedded(devEnv), "bin", "clang.exe" ));
					BuildToolset toolset 	= foundLLVM ? wantToolset : (wantToolset & (~BuildToolset.LLVM));
					AddTarget(ref targets, devEnv, wantPlatform, toolset, wantOptim, wantGraphicsApi);
				}
			}
		}
		
		return targets;
	}
	
	//---------------------------------------------------------------------------------------------
	// Visual Studio Linux Support
	//---------------------------------------------------------------------------------------------
	public virtual List<CustomTarget> CreateVisualStudioLinux()
	{
		List<CustomTarget> targets 	= new List<CustomTarget>();
				
		// Remove all unsupported configuration by this project generator
		DevEnv wantDevEnv 			= (WantedLinuxDevEnv & Settings.SupportLinuxDevEnv);
		Platform wantPlatform 		= (WantedLinuxPlatform & Settings.SupportLinuxPlatform);
		BuildToolset wantToolset	= (WantedLinuxToolset & Settings.SupportLinuxToolset);
		Optimization wantOptim		= (WantedLinuxOptim & Settings.SupportLinuxOptim);
		GraphicsApi wantGraphicsApi	= (WantedLinuxGraphicsApi & Settings.SupportLinuxGraphicsApi);

		// Early out when no supported target detected
		if( wantDevEnv == 0 || wantPlatform == 0 || wantToolset == 0 || wantOptim == 0 || wantGraphicsApi == 0 )
			return targets;
		
		// Generate requested Targets
		foreach (var devEnv in Settings.DevEnvArray)
		{
			if (devEnv.IsVisualStudio() && (devEnv & wantDevEnv) != 0)
			{
				string VSLinuxPath 	= Path.Combine(devEnv.GetVisualStudioDir(), "Common7", "IDE", "VC", "Linux");
				if ( Util.DirectoryExists(VSLinuxPath) ) {
					//TODO Add Visual Studio LLVM support ...
					BuildToolset toolset = wantToolset & (~BuildToolset.LLVM);
					AddTarget(ref targets, devEnv, wantPlatform, toolset, wantOptim, wantGraphicsApi);
				}
			}
		}
		
		return targets;
	}
	
	//---------------------------------------------------------------------------------------------
	// Makefile Windows Support
	//---------------------------------------------------------------------------------------------
	public virtual List<CustomTarget> CreateMakefileWindow()
	{
		List<CustomTarget> targets 	= new List<CustomTarget>();
		
		// Remove all unsupported configuration by this project generator
		DevEnv wantDevEnv 			= (WantedWindowDevEnv & Settings.SupportWindowDevEnv) & DevEnv.make;
		Platform wantPlatform 		= (WantedWindowPlatform & Settings.SupportWindowPlatform);
		BuildToolset wantToolset	= (WantedWindowToolset & Settings.SupportWindowToolset);
		Optimization wantOptim		= (WantedWindowOptim & Settings.SupportWindowOptim);
		GraphicsApi wantGraphicsApi	= (WantedWindowGraphicsApi & Settings.SupportWindowGraphicsApi);
		AddTarget(ref targets, wantDevEnv, wantPlatform, wantToolset, wantOptim, wantGraphicsApi);
		return targets;
	}
	
	//---------------------------------------------------------------------------------------------
	// Makefile Linux Support
	//---------------------------------------------------------------------------------------------
	public virtual List<CustomTarget> CreateMakefileLinux()
	{
		List<CustomTarget> targets 	= new List<CustomTarget>();
		
		// Remove all unsupported configuration by this project generator
		DevEnv wantDevEnv 			= (WantedLinuxDevEnv & Settings.SupportLinuxDevEnv) & DevEnv.make;
		Platform wantPlatform 		= (WantedLinuxPlatform & Settings.SupportLinuxPlatform);
		BuildToolset wantToolset	= (WantedLinuxToolset & Settings.SupportLinuxToolset);
		Optimization wantOptim		= (WantedLinuxOptim & Settings.SupportLinuxOptim);
		GraphicsApi wantGraphicsApi	= (WantedLinuxGraphicsApi & Settings.SupportLinuxGraphicsApi);
		AddTarget(ref targets, wantDevEnv, wantPlatform, wantToolset, wantOptim, wantGraphicsApi);
		return targets;
	}
}

//=============================================================================================
// Build Target
//=============================================================================================
public class CustomTarget : Target
{
	public CustomTarget() { }

	public CustomTarget(DevEnv inDevEnv, Platform inPlatform, Optimization inOptim, BuildToolset inToolset, GraphicsApi inGraphicsApi)
	: base(inPlatform, inDevEnv, inOptim, OutputType.Lib, Blob.NoBlob, BuildSystem.MSBuild)
	{		
		BuildToolset 	= inToolset;
		GraphicsApi 	= inGraphicsApi;
	}
	
	public string GetConfigName()
	{
		return Optimization + "_" + BuildToolset.ToString() + (GraphicsApi != GraphicsApi.None ? "_" + GraphicsApi.ToString() : "");
	}
	
	public bool IsPlatformNameNeeded()
	{
		// Visual Studio allows win32/win64 in name project/config
		// Otherwise, needs to generate a distinct solution/project
		return (DevEnv.IsVisualStudio() && (Platform == Platform.win32 || Platform == Platform.win64)) == false;
	}

	public BuildToolset BuildToolset;
	public GraphicsApi	GraphicsApi;
}

//=============================================================================================
// PROJECT
//=============================================================================================
public class ProjectBase : Project
{
	//---------------------------------------------------------------------------------------------
	// Constructor
	//---------------------------------------------------------------------------------------------
    public ProjectBase(string inName, bool inIsExe, TargetSettings inTargetSettings)
	: base(typeof(CustomTarget))
    {
        Name 					= inName;		
		IsFileNameToLower		= false;
		IsTargetFileNameToLower = false;
		TargetSettings			= inTargetSettings;
		TargetSettings.IsExe 	= inIsExe;
		AddTargets(TargetSettings.Create());
    }
	
	//---------------------------------------------------------------------------------------------
	// For filename ending with name_[suffix].cpp, not in this suffix list
	//---------------------------------------------------------------------------------------------
	public virtual List<string> GetBuildExcludeSuffix(Project.Configuration conf, CustomTarget target)
	{
		List<string> buildFileSuffix = new List<string>();
		
		// Handle Optimisation suffix build exclusion
		Optimization[] optimValues 	= (Optimization[])Enum.GetValues(typeof(Optimization));
		foreach (var optimVal in optimValues){
			if (target.Optimization != optimVal){
				buildFileSuffix.Add(optimVal.ToString());
			}
		}
		
		// Handle Platform suffix build exclusion
		Platform[] platformValues 	= (Platform[])Enum.GetValues(typeof(Platform));
		foreach (var platformVal in platformValues){
			if (target.Platform != platformVal){
				buildFileSuffix.Add(platformVal.ToString());
			}
		}
		if( target.Platform != Platform.win32 && target.Platform != Platform.win64 ){
			buildFileSuffix.Add("win");
		}
		
		return buildFileSuffix;
	}
	
	//---------------------------------------------------------------------------------------------
	// Build configuration
	//---------------------------------------------------------------------------------------------
	[Configure]
	public virtual void ConfigureAll(Project.Configuration conf, CustomTarget target)
	{
		//---------------------------------------------------------------------
		// Generic Projects Options
		conf.Name				= target.GetConfigName();
		conf.Output				= TargetSettings.IsExe ? Project.Configuration.OutputType.Exe : Project.Configuration.OutputType.Lib;
		conf.ProjectFileName	= @"[project.Name]";
		conf.ProjectPath		= @"[solution.Name]";
		conf.ProjectPath 		= target.IsPlatformNameNeeded()	? Path.Combine(TargetSettings.RootPath, "_Projects", "[target.DevEnv]_[target.Platform]" , "[project.Name]")
																: Path.Combine(TargetSettings.RootPath, "_Projects", "[target.DevEnv]" , "[project.Name]");
	
		conf.IntermediatePath	= Path.Combine("[conf.ProjectPath]" , "obj" , "[target.Platform]_[conf.Name]");
		conf.TargetLibraryPath	= Path.Combine("[conf.ProjectPath]" , "lib" , "[target.Platform]_[conf.Name]");
		conf.TargetPath			= Path.Combine(TargetSettings.RootPath, "_bin" , "[target.DevEnv]_[target.BuildToolset]_[target.Platform]");
		conf.TargetFileName 	= "[project.Name]_[target.Optimization]";
		
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
			if( TargetSettings.IsExe == false && (target.Platform == Platform.win32 || target.Platform == Platform.win64)){
				conf.TargetFileFullExtension = ".a";
			}
		}
		
		//---------------------------------------------------------------------
		// LLVM Options

		//---------------------------------------------------------------------
		// Per Target Defines
		bool isWindow = (target.Platform & (Platform.win32 | Platform.win64)) != 0;
		conf.Defines.Add("BUILD_PLATFORM=" + (isWindow ? "Window" : target.Platform.ToString()));
		conf.Defines.Add("BUILD_PLATFORM_" + (isWindow ? "Window" : target.Platform.ToString()));
		conf.Defines.Add("BUILD_OPTIM=" + target.Optimization.ToString());
		conf.Defines.Add("BUILD_OPTIM_" + target.Optimization.ToString());
		conf.Defines.Add("BUILD_GFXAPI=" + target.GraphicsApi.ToString());
		conf.Defines.Add("BUILD_GFXAPI_" + target.GraphicsApi.ToString());
		
		//---------------------------------------------------------------------
		// Platform specific file compilation removal
		List<string> buildFileSuffix = GetBuildExcludeSuffix(conf, target);
		conf.SourceFilesBuildExcludeRegex.Add(@"\.*_(" + string.Join("|", buildFileSuffix.ToArray()) + @")\.cpp$");
	}
	
	public TargetSettings TargetSettings;
}

public class ProjectBaseExe : ProjectBase
{
    public ProjectBaseExe(string inName, TargetSettings inTargetSettings)
	: base(inName, true, inTargetSettings)
	{
	}
}

public class ProjectBaseLib : ProjectBase
{
    public ProjectBaseLib(string inName, TargetSettings inTargetSettings)
	: base(inName, false, inTargetSettings)
	{
	}
}

//=============================================================================================
// SOLUTIONS
//=============================================================================================
public class SolutionBase : Sharpmake.Solution
{
	public SolutionBase(string inName, TargetSettings inTargetSettings)
	: base(typeof(CustomTarget))
	{
		Name					= inName;
		IsFileNameToLower		= false;
		TargetSettings			= inTargetSettings;
		AddTargets(TargetSettings.Create());
	}

	[Configure()]
	public virtual void ConfigureAll(Configuration conf, CustomTarget target)
	{
		conf.Name				= target.GetConfigName();
		conf.SolutionFileName	= "[target.DevEnv]_[solution.Name]" + (target.IsPlatformNameNeeded() ? "_[target.Platform]" : "");
		conf.SolutionPath 		= Path.Combine(TargetSettings.RootPath, "_Projects");
	}
	
	public TargetSettings TargetSettings;
}