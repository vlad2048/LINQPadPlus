using System.Text.Json.Serialization;

namespace LINQPadPlus.BuildSystem._sys.Structs;

sealed record SlnFileState(
	string File,
	Version Version,
	PrjFileState[] Prjs,
	GitStatus GitStatus
);

sealed record PrjFileState(
	string File,
	bool IsPackable,
	PrjRef[] PrjRefs,
	PkgRef[] PkgRefs
)
{
	[JsonIgnore]
	public string Name => Path.GetFileNameWithoutExtension(File);
}


sealed record PrjRef(string File)
{
	[JsonIgnore]
	public string Name => Path.GetFileNameWithoutExtension(File);

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