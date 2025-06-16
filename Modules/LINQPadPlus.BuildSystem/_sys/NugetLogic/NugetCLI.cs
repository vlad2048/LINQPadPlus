using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LINQPad;

namespace LINQPadPlus.BuildSystem._sys.NugetLogic;

static class NugetCLI
{
	const string GlobalPackagesPrefix = "global-packages: ";
	static readonly Lazy<string> globalPackagesFolder = new(() => Cmd.RunAndParse("nuget", null, ["locals", "global-packages", "-list"], ParseGlobalPackagesFolder, null));
	static string GlobalPackagesFolder => globalPackagesFolder.Value;


	public static void Release(string prjFile, bool remote, string apiKey, DumpContainer dc)
	{
		var folder = Path.GetDirectoryName(prjFile)!;
		var folderRelease = Path.Combine(folder, "bin", "Release");
		var name = Path.GetFileNameWithoutExtension(prjFile);

		Delete(folderRelease, "*.nupkg");
		Cmd.Run("dotnet", folder, ["pack"], dc);
		var pkgFile = Directory.GetFiles(folderRelease, "*.nupkg").Single().EnsureFileExists();
		var version = pkgFile.ExtractVersion();

		Cmd.Run("nuget", folder, ["add", pkgFile, "-source", GlobalPackagesFolder, "-expand"], dc);
		Path.Combine(GlobalPackagesFolder, name.ToLowerInvariant(), $"{version}").EnsureFolderExists();

		if (remote)
		{
			Cmd.Run("nuget", folder, ["setapikey", apiKey, "-source", Consts.NugetUrl], dc);
			Cmd.Run("nuget", folder, ["push", pkgFile, "-source", Consts.NugetUrl], dc);
		}

		File.Delete(pkgFile);
	}
	

	static void Delete(string folder, string filter)
	{
		foreach (var file in Directory.GetFiles(folder, filter))
			File.Delete(file);
	}
	static string EnsureFileExists(this string file)
	{
		if (!File.Exists(file))
			throw new FileNotFoundException($"File not found: {file}");
		return file;
	}
	static string EnsureFolderExists(this string folder)
	{
		if (!Directory.Exists(folder))
			throw new FileNotFoundException($"Folder not found: {folder}");
		return folder;
	}
	static Version ExtractVersion(this string file)
	{
		var name = Path.GetFileNameWithoutExtension(file);
		var match = Regex.Match(name, @"^.*\.(\d+\.\d+\.\d+(?:\.\d+)?)$");
		if (match.Success && Version.TryParse(match.Groups[1].Value, out var version))
			return version;
		throw new ArgumentException($"Failed to extract version from '{file}'");
	}






	static string ParseGlobalPackagesFolder(string[] xs) =>
		xs
			.Single(e => e.StartsWith(GlobalPackagesPrefix))
			.RemoveKnownPrefix(GlobalPackagesPrefix);

	static string RemoveKnownPrefix(this string s, string prefix) =>
		s.StartsWith(prefix) switch
		{
			true => s[prefix.Length..],
			false => throw new ArgumentException($"Failed to remove prefix '{prefix}' from '{s}'"),
		};
}