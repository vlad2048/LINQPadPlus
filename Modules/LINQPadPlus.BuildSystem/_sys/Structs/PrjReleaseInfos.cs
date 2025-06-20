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
);


static class PrjReleaseInfosExt
{
	public static bool NeedsNugetPolling(this PrjReleaseInfos release, SlnFileState file) =>
		file.Prjs
			.Where(e => e.IsPackable)
			.Any(e => release.Map[e.Name].VersionReleased == file.Version.Bump());
}