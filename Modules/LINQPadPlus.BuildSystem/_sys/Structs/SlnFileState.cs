using System.Text.Json.Serialization;

namespace LINQPadPlus.BuildSystem._sys.Structs;

sealed record SlnFileState(
	string File,
	Version Version,
	PrjFileState[] Prjs,
	GitStatus GitStatus
)
{
	[JsonIgnore] public string Folder => Path.GetDirectoryName(File)!;

	[JsonIgnore] public string[] IgnoreFolders =>
	[
		Path.Combine(Folder, ".git"),
		/*..Prjs.SelectMany(prj => new[]
		{
			Path.Combine(prj.Folder, "bin"),
			Path.Combine(prj.Folder, "obj"),
		}),*/
	];
}

sealed record PrjFileState(
	string File,
	bool IsPackable,
	PrjRef[] PrjRefs,
	PkgRef[] PkgRefs
)
{
	[JsonIgnore] public string Name => Path.GetFileNameWithoutExtension(File);
	[JsonIgnore] public string Folder => Path.GetDirectoryName(File)!;
}


sealed record PrjRef(string File)
{
	[JsonIgnore] public string Name => Path.GetFileNameWithoutExtension(File);

	public object ToDump() => Name;
}

sealed record PkgRef(string Name, Version Version)
{
	public object ToDump() => new
	{
		Name,
		Version,
	};
}

enum GitStatus
{
	Clean,
	UnStaged,
	UnCommited,
	UnPushed,
}