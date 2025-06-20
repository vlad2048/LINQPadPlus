using LINQPad;
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
	public Version BumpVersion(Sln sln) => Wrap(sln.Version, () =>
	{
		var versionBump = sln.Version.Bump();
		XmlFile.Write(
			sln.DirectoryBuildPropsFile,
			e => e.SetValue("/Project/PropertyGroup/Version", $"{versionBump}")
		);
		return versionBump;
	});

	public void Push(Sln sln, string commitMsg) => Wrap(() =>
	{
		GitOps.PushChanges(sln.Folder, commitMsg, dc);
	});

	public void Release(Sln sln) => Wrap(() =>
	{
		if (!sln.IsReleasable(out var reason))
			throw new ArgumentException($"Impossible. Solution is not releasable: {reason}");
		foreach (var prj in sln.Prjs.WhereA(e => e.Status is PrjStatus.Ready))
		{
			NugetCLI.Release(prj.File, true, nugetApiKey, dc);
		}

		GitOps.TagCreate(sln.Folder, sln.Version, dc);
	});

	public void ReleasePrjLocally(Prj prj) => Wrap(() =>
	{
		NugetCLI.Release(prj.File, false, "", dc);
	});

	void Wrap(Action action)
	{
		try
		{
			dc.ClearContent();
			action();
		}
		catch (Exception ex)
		{
			dc.AppendContent(ex);
		}
	}

	T Wrap<T>(T def, Func<T> action)
	{
		try
		{
			dc.ClearContent();
			return action();
		}
		catch (Exception ex)
		{
			dc.AppendContent(ex);
			return def;
		}
	}
}

file static class ExecUtils
{
	public static Version Bump(this Version e) => new(e.Major, e.Minor, e.Build + 1);
}
