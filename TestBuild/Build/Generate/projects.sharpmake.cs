using Sharpmake;
using System;
using System.IO; // Path
using System.Collections.Generic; //List

// Custom Target generation settings
public class MyTargetSettings : TargetSettings
{
	public MyTargetSettings(){
		// Adding a GFX api
		WantedWindowGraphicsApi	|= GraphicsApi.DirectX12;
		WantedLinuxGraphicsApi	|= GraphicsApi.Vulkan;
	}
}

[Generate]
public class ProjectExampleLib : ProjectBaseLib
{
    public ProjectExampleLib()
	: base("ExampleLib", new MyTargetSettings())
    {
        SourceRootPath = Path.Combine(TargetSettings.RootPath, "Sources", "ExampleLib");
    }
}

[Generate]
public class ProjectExample : ProjectBaseExe
{
    public ProjectExample()
	: base("Example", new MyTargetSettings())
    {
        SourceRootPath = Path.Combine(TargetSettings.RootPath, "Sources", "Example");
    }
	
	// Demonstration of including a source file but not compiling
	public override List<string> GetBuildExcludeSuffix(Project.Configuration conf, CustomTarget target)
	{
		List<string> BuildFileSuffix = base.GetBuildExcludeSuffix(conf, target);
		// Can add extra rejection rules here
		return BuildFileSuffix;
	}
	
	public override void ConfigureAll(Configuration conf, CustomTarget target)
	{
		base.ConfigureAll(conf, target);
		conf.AddPublicDependency<ProjectExampleLib>(target);
	}
	
}