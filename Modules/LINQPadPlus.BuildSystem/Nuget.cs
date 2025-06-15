using LINQPadPlus.BuildSystem._sys.NugetLogic;

namespace LINQPadPlus.BuildSystem;


public interface ISt;
public sealed record UnreleasedSt(bool NeverReleased) : ISt;
public sealed record ReleasedCurrentSt(bool IsPending) : ISt;
public sealed record ErrorReleasedFutureSt : ISt;



public static class Nuget
{
	static readonly Lazy<string> searchUrl = new(() => NugetAPI.GetIndex().Resources.First(e => e.Type == "SearchQueryService").Id);
	static string SearchUrl => searchUrl.Value;

	public static NugetSearchQueryService Query(string search, bool preRelease) =>
		NugetAPI.Search(SearchUrl, search, preRelease);
}