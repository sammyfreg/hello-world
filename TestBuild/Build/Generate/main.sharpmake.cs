using Sharpmake;
using System;
[module: Sharpmake.Include("shared.sharpmake.cs")]
[module: Sharpmake.Include("projects.sharpmake.cs")]

//=================================================================================================
// SOLUTIONS
//=================================================================================================
[Generate]
public class SolutionExample : SolutionBase
{
	public SolutionExample()
	: base("SolutionExample", new MyTargetSettings())
    {
    }
    
    public override void ConfigureAll(Solution.Configuration conf, CustomTarget target)
    {
		base.ConfigureAll(conf, target);
        conf.AddProject<ProjectExample>(target);
    }
}


public static class main
{
	[Sharpmake.Main]
	public static void SharpmakeMain(Sharpmake.Arguments arguments)
	{
		arguments.Generate<SolutionExample>();
	}
}



