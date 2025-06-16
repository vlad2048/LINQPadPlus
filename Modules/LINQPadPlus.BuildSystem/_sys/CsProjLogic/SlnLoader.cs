using System.Text.RegularExpressions;
using LINQPad;
using LINQPadPlus.BuildSystem._sys.CsProjLogic.Structs;
using LINQPadPlus.BuildSystem._sys.CsProjLogic.Xml;
using LINQPadPlus.BuildSystem._sys.GitLogic;
using LINQPadPlus.BuildSystem._sys.Structs;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys.CsProjLogic;



static class SlnLoader
{
	public static SlnFileState Load(string slnFile, DumpContainer dc) => new(
		
		slnFile,
		
		XmlFile.Read(
			slnFile.GetDirectoryBuildPropsFile(),
			r => r.GetValue("/Project/PropertyGroup/Version")
				.AsVersion().Ensure("Failed to parse solution Version")
		),
	
		File.ReadAllLines(slnFile)
			.ExtractPrjFilesRel()
			.Select(prjFileRel => prjFileRel.ResolvePath(slnFile))
			.SelectA(prjFile => new PrjFileState(
				
				prjFile,
				
				XmlFile.Read(
					prjFile,
					r => r.GetValueFlag(PrjFlag.IsPackable)
				),
				
				XmlFile.Read(
						prjFile,
						r => r.GetItems(
							"//ProjectReference",
							e => e.GetAttr("Include")
						)
					)
					.Select(prjRefFile => prjRefFile.ResolvePath(prjFile))
					.SelectA(prjRefFile => new PrjRef(
						prjRefFile
					)),
				
				XmlFile.Read(prjFile, r => r.GetItems(
					"//PackageReference",
					e => new PkgRef(
						e.GetAttr("Include"),
						e.GetAttr("Version")
							.AsVersion().Ensure($"Failed to parse version: '{e.GetAttr("Version")}' in {prjFile}")
					)
				))
				
			)),
		
		GitOps.GetStatus(Path.GetDirectoryName(slnFile)!, dc)
		
	);

	static string GetDirectoryBuildPropsFile(this string slnFile) => Path.Combine(Path.GetDirectoryName(slnFile)!, "Directory.Build.props");


	static string ResolvePath(this string prjFileRel, string refFile) =>
		Path.Combine(Path.GetDirectoryName(refFile)!, prjFileRel)
			.EnsureExists(refFile);

	static string EnsureExists(this string prjFile, string refFile)
	{
		if (!File.Exists(prjFile))
			throw new ArgumentException($"Failed to find .csproj file in the solution: '{prjFile}' referenced in: '{refFile}'");
		return prjFile;
	}

	static readonly Regex prjRegex = new(@"Project\(""\{.+\}""\)\s*=\s*"".+"",\s*""([^""]+\.csproj)""", RegexOptions.Compiled);

	static string[] ExtractPrjFilesRel(this string[] slnLines)
	{
		var files = new List<string>();
		foreach (var line in slnLines)
		{
			var match = prjRegex.Match(line);
			if (match.Success)
				files.Add(match.Groups[1].Value);
		}
		return [..files];
	}
}