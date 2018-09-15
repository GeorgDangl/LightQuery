using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild
{
    public static int Main () => Execute<Build>(x => x.Compile);

    [Solution] readonly Solution Solution;

    Target Clean => _ => _
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
        });

    private Target Pack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
        });
}
