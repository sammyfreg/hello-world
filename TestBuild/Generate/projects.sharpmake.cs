using Sharpmake;
using System;

[Generate]
public class ProjectExampleLib : ProjectBaseLib
{
    public ProjectExampleLib()
	: base("ExampleLib", true)
    {
        SourceRootPath = @"[project.SharpmakeCsPath]/../Sources/ExampleLib";
    }
}

[Generate]
public class ProjectExample : ProjectBaseExe
{
    public ProjectExample()
	: base("Example", true)
    {
        SourceRootPath = @"[project.SharpmakeCsPath]/../Sources/Example";
    }
	
	public override void ConfigureAll(Configuration conf, CustomTarget target)
	{
		base.ConfigureAll(conf, target);
		conf.AddPublicDependency<ProjectExampleLib>(target);
	}
	
}