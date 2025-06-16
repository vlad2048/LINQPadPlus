using System.Text.Json;
using System.Text.Json.Serialization;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys.Structs;


sealed record Sln(
	string File,
	Version Version,
	Prj[] Prjs,
	GitStatus GitStatus,
	[property: JsonIgnore]
	UserActions Actions
)
{
	[JsonIgnore]
	public string Name => Path.GetFileNameWithoutExtension(File);


	public static readonly IEqualityComparer<Sln> EqualityComparer = EqualityUtils.Make<Sln, string>(e => JsonSerializer.Serialize(e));
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







/*return new Sln(
	"LINQPadPlus",
	new Version(0, 0, 10),
	[
		new Prj("LINQPadPlus", false, [], [], PrjStatus.NotPackable, null, null),
		new Prj("LINQPadPlus.Plotly", true, [], [], PrjStatus.Never, null, null),
		new Prj("LINQPadPlus.Tabulator", true, [], [], PrjStatus.Ready, new Version(0, 0, 9), null),
		new Prj("LINQPadPlus.BuildSystem", true, [], [], PrjStatus.Pending, new Version(0, 0, 9), new Version(0, 0, 10)),
		new Prj("LINQPadPlus.Other", true, [], [], PrjStatus.UptoDate, new Version(0, 0, 10), null),
		new Prj("LINQPadPlus.Milou", true, [], [], PrjStatus.ERROR, new Version(0, 0, 16), null),
	],
	GitStatus.UnStaged,
	new Dictionary<string, Version>()
);*/
