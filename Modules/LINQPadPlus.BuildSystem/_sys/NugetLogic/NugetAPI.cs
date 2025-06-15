using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LINQPadPlus.BuildSystem._sys.NugetLogic;




public record NugetIndex(
	Version Version,
	NugetIndex.Resource[] Resources
)
{
	public record Resource(
		[property: JsonPropertyName("@id")] string @Id,
		[property: JsonPropertyName("@type")] string @Type,
		string Comment
	);
}



public record NugetSearchQueryService(
	int TotalHits,
	NugetSearchQueryService.Item[] Data
)
{
	public record Item(
		string Id,
		string[] Authors,
		Item.ItemVersion[] Versions
	)
	{
		public record ItemVersion(
			Version Version
		);
	}
}




static class NugetAPI
{
	const string Url = "https://api.nuget.org/v3/index.json";
	static readonly HttpClient client = new();


	public static NugetIndex GetIndex() =>
		Get<NugetIndex>(
			Url,
			b => b
		);
	
	public static NugetSearchQueryService Search(string url, string search, bool preRelease) =>
		Get<NugetSearchQueryService>(
			url,
			b => b
				.Set("q", search)
				.Set("prerelease", preRelease)
		);


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