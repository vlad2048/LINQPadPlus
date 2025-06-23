using System.Text.Json;
using System.Text.Json.Serialization;
using LINQPadPlus.BuildSystem._sys.Utils;
using C = LINQPadPlus.BuildSystem.DisplayConsts;

namespace LINQPadPlus.BuildSystem._sys.Structs;


sealed record Sln(
	string File,
	Version Version,
	Prj[] Prjs,
	GitStatus GitStatus
)
{
	[JsonIgnore] public string Name => Path.GetFileNameWithoutExtension(File);
	[JsonIgnore] public string Folder => Path.GetDirectoryName(File) ?? throw new ArgumentException($"Solution file has no folder: '{File}'");
	[JsonIgnore] public string DirectoryBuildPropsFile => File.GetDirectoryBuildPropsFile();
	public static readonly IEqualityComparer<Sln> EqualityComparer = EqualityUtils.Make<Sln, string>(e => JsonSerializer.Serialize(e));
}


public enum PrjStatus
{
	/// <summary>
	/// IsPackable=False
	/// </summary>
	NotPackable,

	/// <summary>
	/// Nuget.Version &lt; Sln.Version
	/// </summary>
	Ready,

	/// <summary>
	/// Nuget.CachedVersion &gt; Nuget.Version
	/// </summary>
	Pending,

	/// <summary>
	/// Nuget.Version = Sln.Version
	/// </summary>
	UptoDate,

	/// <summary>
	/// Nuget.Version &gt; Sln.Version
	/// </summary>
	ERROR,
}

sealed record Prj(
	string File,
	bool IsPackable,
	PrjRef[] PrjRefs,
	PkgRef[] PkgRefs,
	PrjStatus Status,
	Version? VersionReleased,
	Version? VersionPending
)
{
	[JsonIgnore]
	public string Name => Path.GetFileNameWithoutExtension(File);

	public object ToDump() => new
	{
		Name,
		Status,
		Version = (VersionReleased, VersionPending) switch
		{
			(null, null) => "unreleased",
			(null, not null) => $"pending {VersionPending}",
			(not null, null) => $"{VersionReleased}",
			(not null, not null) when VersionPending > VersionReleased => $"{VersionReleased} (pending {VersionPending})",
			_ => throw new ArgumentException($"Invalid versions.  Released:{VersionReleased}  Pending:{VersionPending}"),
		},
		PrjRefs,
		PkgRefs,
	};
}



sealed record ReleaseIssue(
	string Text,
	bool IsGood
);


static class SlnUtils
{
	public static Maybe<ReleaseIssue> GetReleaseIssue(this Sln sln)
	{
		Maybe<ReleaseIssue> ErrBad(string text) => May.Some(new ReleaseIssue(text, false));
		Maybe<ReleaseIssue> ErrGood(string text) => May.Some(new ReleaseIssue(text, true));

		if (sln.GitStatus is not GitStatus.Clean) return ErrBad("Git needs to be clean");

		var prjs = sln.Prjs.WhereA(e => e.IsPackable);
		if (prjs.Length == 0) return ErrBad("No packable projects");

		if (prjs.Any(PrjStatus.ERROR)) return ErrBad("Projects with errors");
		if (prjs.Any(PrjStatus.Pending)) return ErrBad("Pending projects");
		if (!prjs.Any(PrjStatus.Ready))
		{
			if (prjs.Count(e => e.Status is PrjStatus.UptoDate) > 0)
				return ErrGood("All projects are up to date");
			else
				return ErrBad("No projects are ready");
		}

		return May.None<ReleaseIssue>();
	}
	
	static bool Any(this Prj[] prjs, PrjStatus status) => prjs.Any(e => e.Status == status);

	
	/*
	public static bool IsReleasable(this Sln sln, [NotNullWhen(false)] out string? reason)
	{
		if (sln.GitStatus is not GitStatus.Clean)
		{
			reason = "Git needs to be clean";
			return false;
		}

		var prjs = sln.Prjs.WhereA(e => e.IsPackable);
		if (prjs.Length == 0)
		{
			reason = "No packable projects";
			return false;
		}
		
		var prjsIssues = prjs.WhereA(e => e.Status is PrjStatus.ERROR);
		if (prjsIssues.Length > 0)
		{
			reason = $"Projects with errors: {string.Join(", ", prjsIssues.Select(e => e.Name))}";
			return false;
		}

		var prjsReleasable = prjs.WhereA(e => e.Status is PrjStatus.Ready);
		if (prjsReleasable.Length == 0)
		{
			reason = "No projects are Ready";
			return false;
		}

		reason = null;
		return true;
	}
	*/
}


