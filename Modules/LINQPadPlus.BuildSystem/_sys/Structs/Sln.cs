using System.Text.Json;
using System.Text.Json.Serialization;
using LINQPadPlus.BuildSystem._sys.Utils;

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
	/// Has never been released to nuget yet
	/// </summary>
	Never,

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




static class SlnUtils
{
	public static bool IsReleasable(this Sln sln, out string? reason)
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

		var prjsNotReady = prjs.WhereA(e => e.Status is not PrjStatus.Ready);
		if (prjsNotReady.Length > 0)
		{
			reason = $"Projects not ready: {string.Join(", ", prjsNotReady.Select(e => e.Name))}";
			return false;
		}

		reason = null;
		return true;
	}
}


