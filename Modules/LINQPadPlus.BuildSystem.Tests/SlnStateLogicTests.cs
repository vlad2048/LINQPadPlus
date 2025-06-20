using LINQPadPlus.BuildSystem._sys;
using LINQPadPlus.BuildSystem._sys.Structs;
using Shouldly;

namespace LINQPadPlus.BuildSystem.Tests;

class SlnStateLogicTests
{
	static readonly Version v1 = new(0, 0, 1);
	static readonly Version v2 = new(0, 0, 2);
	static readonly Version v3 = new(0, 0, 3);

	static readonly PrjFileState A = Prj("A", false);
	static readonly PrjFileState B = Prj("B", true);
	static readonly PrjFileState C = Prj("C", true);
	static readonly PrjFileState D = Prj("D", true);
	static readonly PrjFileState E = Prj("E", true);
	static readonly PrjFileState F = Prj("F", true);

	[Test]
	public void Simple() => Run(
			File(v2, [A, B, C, D, E, F]),
			Nuget(
				[
					(C, v1),
					(D, v1),
					(E, v2),
					(F, v3),
				],
				[
					(D, v2),
				]
			)
		)
		.Check([
			Res(A, PrjStatus.NotPackable),
			Res(B, PrjStatus.Ready),
			Res(C, PrjStatus.Ready, v1),
			Res(D, PrjStatus.Pending, v1, v2),
			Res(E, PrjStatus.UptoDate, v2),
			Res(F, PrjStatus.ERROR, v3),
		]);
	
	
	static PrjReleaseInfos Run(SlnFileState file, SlnNugetState nuget) => SlnStateLogic.GetReleaseInfos_From_FileAndNugetState(file, nuget);

	static SlnFileState File(Version version, PrjFileState[] prjs) => new("", version, prjs, GitStatus.Clean);
	static PrjFileState Prj(string name, bool isPackable) => new(name, isPackable, [], []);

	static SlnNugetState Nuget((PrjFileState, Version)[] released, (PrjFileState, Version)[] pending) => new(
		released.ToDictionary(e => e.Item1.Name, e => e.Item2),
		pending.ToDictionary(e => e.Item1.Name, e => e.Item2)
	);

	static ResRec Res(PrjFileState prj, PrjStatus status, Version? versionReleased = null, Version? versionPending = null) => new(prj.Name, status, versionReleased, versionPending);
}


sealed record ResRec(string Name, PrjStatus Status, Version? VersionReleased, Version? VersionPending);


file static class ShouldUtils
{
	public static void Check(this PrjReleaseInfos actRaw, ResRec[] expRaw)
	{
		var act = actRaw.Map.OrderBy(kv => kv.Key).ToArray().Print("Actual");
		var exp = expRaw.ToDictionary(e => e.Name, e => new PrjReleaseNfo(e.Status, e.VersionReleased, e.VersionPending)).OrderBy(kv => kv.Key).ToArray().Print("Expected");
		act.ShouldBeEquivalentTo(exp);
	}

	static KeyValuePair<string, PrjReleaseNfo>[] Print(this KeyValuePair<string, PrjReleaseNfo>[] xs, string title)
	{
		LogTitle(title);
		foreach (var (name, nfo) in xs)
			Log($"    {name}    {nfo.Status.Fmt()}    released: {nfo.VersionReleased.Fmt()}    pending: {nfo.VersionPending.Fmt()}");
		return xs;
	}

	static string Fmt(this PrjStatus e) => $"{e}".PadRight(11);

	static string Fmt(this Version? e) => (e switch
	{
		not null => $"{e}",
		null => "_",
	}).PadRight(5);

	static void Log(string s) => Console.WriteLine(s);

	static void LogTitle(string s)
	{
		Log(s);
		Log(new string('=', s.Length));
	}
}
