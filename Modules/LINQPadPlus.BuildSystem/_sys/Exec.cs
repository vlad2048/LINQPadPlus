using LINQPad;
using LINQPad.FSharpExtensions;
using LINQPadPlus.BuildSystem._sys.CsProjLogic;
using LINQPadPlus.BuildSystem._sys.GitLogic;
using LINQPadPlus.BuildSystem._sys.NugetLogic;
using LINQPadPlus.BuildSystem._sys.Structs;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys;

interface IExec
{
	Version BumpVersion(Sln sln);
	void Push(Sln sln, string commitMsg);
	void Release(Sln sln);
	void ReleasePrjLocally(Prj prj);
}

sealed class TestExec : IExec
{
	public Version BumpVersion(Sln sln) => sln.Version.Bump();
	public void Push(Sln sln, string commitMsg)
	{
	}
	public void Release(Sln sln)
	{
	}
	public void ReleasePrjLocally(Prj prj)
	{
	}
}

sealed class Exec(DumpContainer dc, string nugetApiKey) : IExec
{
	public Version BumpVersion(Sln sln)
	{
		var versionBump = sln.Version.Bump();
		XmlFile.Write(
			sln.DirectoryBuildPropsFile,
			e => e.SetValue("/Project/PropertyGroup/Version", $"{versionBump}")
		);
		return versionBump;
	}

	public void Push(Sln sln, string commitMsg) =>
		GitOps.PushChanges(sln.Folder, commitMsg, dc);

	public void Release(Sln sln)
	{
		if (!sln.IsReleasable(out var reason))
			throw new ArgumentException($"Impossible. Solution is not releasable: {reason}");
		foreach (var prj in sln.Prjs.WhereA(e => e.IsPackable))
		{
			NugetCLI.Release(prj.File, true, nugetApiKey, dc);
		}
		GitOps.TagCreate(sln.Folder, sln.Version, dc);
	}

	public void ReleasePrjLocally(Prj prj) =>
		NugetCLI.Release(prj.File, false, "", dc);
}

file static class ExecUtils
{
	public static Version Bump(this Version e) => new(e.Major, e.Minor, e.Build + 1);
}
