using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using LINQPadPlus.BuildSystem._sys.Structs;

namespace LINQPadPlus.BuildSystem._sys.NugetLogic;


// https://learn.microsoft.com/en-us/nuget/api/search-query-service-resource
static class NugetAPI
{
	public static SlnNugetState GetState(SlnFileState sln, string nugetUrl) => new(
		GetReleasedVersions(sln, nugetUrl),
		GetPendingVersions(sln)
	);



	public static void CacheUpdate(string name, Version version) => cache[name] = version;



	static IReadOnlyDictionary<string, Version> GetReleasedVersions(SlnFileState sln, string nugetUrl)
	{
		searchUrl ??= GetIndex(nugetUrl).Resources.First(e => e.Type == "SearchQueryService").Id;
		return
			(
				from prj in sln.Prjs
				where prj.IsPackable
				from item in Search(searchUrl, prj.Name, nugetUrl).Data
				let maxVersion = item.Versions.Select(e => e.Version).Max()
				select new
				{
					Name = item.Id,
					Version = maxVersion,
				}
			)
			.Distinct()
			.ToDictionary(e => e.Name, e => e.Version);
	}

	static IReadOnlyDictionary<string, Version> GetPendingVersions(SlnFileState sln) =>
		(
			from prj in sln.Prjs
			where prj.IsPackable
			where cache.ContainsKey(prj.Name)
			select new
			{
				prj.Name,
				Version = cache[prj.Name],
			}
		)
		.ToDictionary(e => e.Name, e => e.Version);

	




	static readonly HttpClient client = new();
	static string? searchUrl;
	static readonly Dictionary<string, Version> cache = [];
	


	static NugetIndex GetIndex(string nugetUrl) =>
		Get<NugetIndex>(
			nugetUrl,
			b => b
		);
	
	public static NugetSearchQueryService Search(string url, string search, string nugetUrl)
	{
		searchUrl ??= GetIndex(nugetUrl).Resources.First(e => e.Type == "SearchQueryService").Id;
		return Get<NugetSearchQueryService>(
			url,
			b => b
				.Set("q", search)
				.Set("prerelease", false) // setting it to true does not include pending packages
		);
	}


	sealed record NugetIndex(
		Version Version,
		NugetIndex.Resource[] Resources
	)
	{
		public sealed record Resource(
			[property: JsonPropertyName("@id")] string @Id,
			[property: JsonPropertyName("@type")] string @Type,
			string Comment
		);
	}



	public sealed record NugetSearchQueryService(
		int TotalHits,
		NugetSearchQueryService.Item[] Data
	)
	{
		public sealed record Item(
			string Id,
			string[] Authors,
			Item.ItemVersion[] Versions
		)
		{
			public sealed record ItemVersion(
				Version Version
			);
		}
	}








	interface IBuild
	{
		IBuild Set(string name, bool val);
		IBuild Set(string name, int val);
		IBuild Set(string name, string val);
	}

	static T Get<T>(string url, Func<IBuild, IBuild> f)
	{
		var build = new Build(url);
		f(build);
		return JsonUtils.Deser<T>(client.GetStringAsync(build.FullUrl).Result);
	}

	sealed record Build : IBuild
	{
		readonly StringBuilder sb;
		bool isFirst = true;
		public string FullUrl => sb.ToString();
		public Build(string url) => sb = new StringBuilder(url);

		public IBuild Set(string name, bool val) => Set(name, $"{val}".ToLowerInvariant());
		public IBuild Set(string name, int val) => Set(name, $"{val}");
		public IBuild Set(string name, string val)
		{
			var ch = isFirst ? "?" : "&";
			isFirst = false;
			sb.Append($"{ch}{name}={Uri.EscapeDataString(val)}");
			return this;
		}
	}
}



file static class JsonUtils
{
	static readonly JsonSerializerOptions jsonOpt = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	public static T Deser<T>(string s) => JsonSerializer.Deserialize<T>(s, jsonOpt)!;
}