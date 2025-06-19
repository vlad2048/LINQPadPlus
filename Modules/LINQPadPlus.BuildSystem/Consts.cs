namespace LINQPadPlus.BuildSystem;

static class Consts
{
	public const string NugetUrl = "https://api.nuget.org/v3/index.json";

	public static readonly TimeSpan FileDebouncePeriod = TimeSpan.FromMilliseconds(100);
	public static readonly TimeSpan NugetDebouncePeriod = TimeSpan.FromSeconds(20);
	
	//public static readonly TimeSpan NugetPollingPeriod = TimeSpan.FromSeconds(20);
	//public static readonly TimeSpan GitPollingPeriod = TimeSpan.FromSeconds(10);
}