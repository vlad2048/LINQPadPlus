using System.Text.Json.Serialization;
using LINQPadPlus.BuildSystem._sys.Reading;

namespace LINQPadPlus.BuildSystem;

public sealed record Sln(
	string File,
	Version Version,
	Prj[] Prjs
)
{
	[JsonIgnore]
	public string Name => Path.GetFileNameWithoutExtension(File);
	
	public static Sln Load(string slnFile) => SlnLoader.Load(slnFile);

	public object ToDump() => new
	{
		Name,
		Version,
		Prjs,
	};
}

public sealed record Prj(
	string File,
	bool IsPackable,
	PrjRef[] PrjRefs,
	PkgRef[] PkgRefs
)
{
	[JsonIgnore]
	public string Name => Path.GetFileNameWithoutExtension(File);
	
	public object ToDump() => new
	{
		Name,
		IsPackable,
		PrjRefs,
		PkgRefs,
	};
}

public sealed record PrjRef(string File)
{
	[JsonIgnore]
	public string Name => Path.GetFileNameWithoutExtension(File);

	public object ToDump() => Name;
}

public sealed record PkgRef(string Name, Version Version)
{
	public object ToDump() => new
	{
		Name,
		Version,
	};
}
