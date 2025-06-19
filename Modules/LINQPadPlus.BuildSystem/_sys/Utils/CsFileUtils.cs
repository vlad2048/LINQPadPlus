namespace LINQPadPlus.BuildSystem._sys.Utils;

static class CsFileUtils
{
	public static string GetDirectoryBuildPropsFile(this string slnFile) => Path.Combine(Path.GetDirectoryName(slnFile)!, "Directory.Build.props");
}