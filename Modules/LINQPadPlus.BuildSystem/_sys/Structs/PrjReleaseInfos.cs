using System.Text.Json.Serialization;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys.Structs;

sealed record PrjReleaseNfo(
	PrjStatus Status,
	Version? VersionReleased,
	Version? VersionPending
);

sealed record PrjReleaseInfos(
	IReadOnlyDictionary<string, PrjReleaseNfo> Map
)
{
	[JsonIgnore]
	public bool NeedsNugetPolling => Map.Values.Any(e => e.Status is PrjStatus.Pending);
}


static class PrjReleaseInfosExt
{
	public static string[] Get(this PrjReleaseInfos infos, Func<PrjStatus, bool> pred) =>
		infos.Map
			.Where(kv => pred(kv.Value.Status))
			.SelectA(kv => kv.Key);
}