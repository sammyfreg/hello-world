using System.IO; // for Path.Combine
using Sharpmake; // contains the entire Sharpmake object library.

public class MyTarget : Target
{
	public MyTarget() { }

	public MyTarget(
		Platform platform,
		DevEnv devEnv,
		Optimization optimization,
		OutputType outputType = OutputType.Lib,
		Blob blob = Blob.NoBlob,
		BuildSystem buildSystem = BuildSystem.MSBuild
	)
	: base(platform, devEnv, optimization, outputType, blob, buildSystem)
	{
	}
}
		
// Represents the project that will be generated by Sharpmake and that contains
// the sample C++ code.
[Generate]
public class BasicsProject : Project
{
    public BasicsProject()
	: base(typeof(MyTarget))
    {
        // The name of the project in Visual Studio. The default is the name of
        // the class, but you usually want to override that.
        Name = "Basics";

        // The directory that contains the source code we want to build is the
        // same as this one. This string essentially means "the directory of
        // the script you're reading right now."
        SourceRootPath = @"[project.SharpmakeCsPath]/../Sources/";

        // Specify the targets for which we want to generate a configuration
        // for. Instead of creating multiple targets manually here, we can
        // use the binary OR operator to define multiple targets at once.
        // Sharpmake will generate all combinations possible and generate a
        // target for it.
        //
        // The code below is the same as creating 2 separate targets having
        // those flag combinations:
        //    * Platform.win64, DevEnv.vs2022, Optimization.Debug
        //    * Platform.win64, DevEnv.vs2022, Optimization.Release
        AddTargets(new MyTarget(
            // we want a target that builds 64-bit Windows.
            Platform.win64,

            // we only care about Visual Studio 2022. (Edit as needed.)
            DevEnv.vs2022 | DevEnv.make,

            // of course, we want a debug and a release configuration.
            Optimization.Debug | Optimization.Release));
    }
	
	// Sets the properties of each configuration (conf) according to the target.
	//
	// This method is called once for every target specified by AddTargets. Since
	// we only want vs2015 targets and we want 32- and 64-bit targets, each having
	// a debug and a release version, we have 1 x 2 x 2 targets to configure, so it
	// will be called 4 times.
	//
	// If we had instead specified vs2012 | vs2015 | vs2017 it would have been
	// called 12 times. (3 x 2 x 2)
	[Configure]
	public void ConfigureAll(Project.Configuration conf, MyTarget target)
	{
		//conf.Name				= @"[target.Compiler]_[target.Optimization]";
		//conf.ProjectFileName	= @"[project.Name]";
		//conf.TargetFileSuffix	= @"_[target.Optimization]";
		//conf.ProjectPath		= NetImguiTarget.GetPath(@"\_projects\[target.DevEnv]");
		//conf.TargetPath			= NetImguiTarget.GetPath( mIsExe	? @"\_Bin\[target.DevEnv]_[target.Compiler]_[target.Platform]" 
		//															: @"\_generated\Libs\[target.DevEnv]_[target.Compiler]_[target.Platform]");
		//conf.IntermediatePath	= NetImguiTarget.GetPath(@"\_intermediate\[target.DevEnv]_[target.Compiler]_[target.Platform]_[target.Optimization]\[project.Name]");
		//conf.Output				= mIsExe ? Project.Configuration.OutputType.Exe : Project.Configuration.OutputType.Lib;
		//conf.IncludePaths.Add(NetImguiTarget.GetPath(ProjectImgui.sDefaultPath) + @"\backends");
		//conf.IntermediatePath	= DevEnv.ToString()
		//conf.Name				= target.DevEnv.ToString() + @"_[target.Optimization]";
		//conf.ProjectFileName	= conf.Name;
		//conf.ProjectName		= conf.Name;
		//conf.TargetPath			= 
		
		// Specify where the generated project will be. Here we generate the
		// vcxproj in a /generated directory.
		conf.ProjectPath = @"[project.SharpmakeCsPath]/../_Solution/Projects";
		conf.ProjectPath += target.DevEnv == DevEnv.make ? "_make" : "_vs";
	}
}

// Represents the solution that will be generated and that will contain the
// project with the sample code.
public class BasicSolution : Solution
{
    public BasicSolution(DevEnv devEnv)
	: base(typeof(MyTarget))
    {
	    // The name of the solution.
        Name = "Basics";

        // As with the project, define which target this solution builds for.
        // It's usually the same thing.
        AddTargets(new MyTarget(Platform.win64, devEnv, Optimization.Debug | Optimization.Release));
    }

    // Configure for all 4 generated targets. Note that the type of the
    // configuration object is of type Solution.Configuration this time.
    // (Instead of Project.Configuration.)
    [Configure]
    public void ConfigureAll(Solution.Configuration conf, MyTarget target)
    {
        // Puts the generated solution in the /generated folder too.
        conf.SolutionPath = @"[solution.SharpmakeCsPath]/../_Solution";
		//if( target.DevEnv == DevEnv.make ){
		//	conf.SolutionPath += "/Make";
		//}
			
        // Adds the project described by BasicsProject into the solution.
        // Note that this is done in the configuration, so you can generate
        // solutions that contain different projects based on their target.
        //
        // You could, for example, exclude a project that only supports 64-bit
        // from the 32-bit targets.
        conf.AddProject<BasicsProject>(target);
    }
}

[Generate] public class BasicSolution_VS 	: BasicSolution { public BasicSolution_VS() : base(DevEnv.vs2022){} }
[Generate] public class BasicSolution_Make 	: BasicSolution { public BasicSolution_Make() : base(DevEnv.make){} }


public static class Main
{
    [Sharpmake.Main]
    public static void SharpmakeMain(Sharpmake.Arguments arguments)
    {
        // Tells Sharpmake to generate the solution described by
        // BasicsSolution.
        arguments.Generate<BasicSolution_VS>();
		arguments.Generate<BasicSolution_Make>();
    }
}