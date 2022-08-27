using Sharpmake;
using System;
using System.IO; // Path

[Generate]
public class ProjectExampleLib : ProjectBaseLib
{
    public ProjectExampleLib()
	: base("ExampleLib", true)
    {
        SourceRootPath = Path.Combine(Settings.RootPath, "Sources", "ExampleLib");
    }
}

[Generate]
public class ProjectExample : ProjectBaseExe
{
    public ProjectExample()
	: base("Example", true)
    {
        SourceRootPath = Path.Combine(Settings.RootPath, "Sources", "Example");
    }
	
	public override void ConfigureAll(Configuration conf, CustomTarget target)
	{
		base.ConfigureAll(conf, target);
		conf.AddPublicDependency<ProjectExampleLib>(target);
	}
	
}