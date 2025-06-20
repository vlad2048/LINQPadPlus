namespace LINQPadPlus.BuildSystem._sys.Utils;

static class VersionExt
{
	public static Version Bump(this Version e) => new(e.Major, e.Minor, e.Build + 1);
}