using LINQPad;
using LINQPadPlus.BuildSystem._sys.Structs;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys;

static class SlnStateLogic
{
	public static PrjReleaseInfos GetReleaseInfos_From_FileAndNugetState(
		SlnFileState sln,
		SlnNugetState nuget
	)
	{
		var (released, cached) = nuget;


		var xs = sln.Prjs.SelectA(e => e.Name);

		var xsNotPackable = (
			from x in xs
			let prj = sln.Prjs.Single(e => e.Name == x)
			where !prj.IsPackable
			select x
		).ToArray();
		xs = xs.Del(xsNotPackable);

		var xsErr = (
			from x in xs
			let maxVer = GetVersionInBoth(x, released, cached)
			where maxVer > sln.Version
			select x
		).ToArray();
		xs = xs.Del(xsErr);

		var xsPending = (
			from x in xs
			let releasedVer = released.GetValueOrDefault(x)
			let cachedVer = cached.GetValueOrDefault(x)
			where cachedVer == sln.Version
			where cachedVer > releasedVer
			select x
		).ToArray();
		xs = xs.Del(xsPending);

		var xsNever = (
			from x in xs
			where !released.ContainsKey(x)
			where !cached.ContainsKey(x)
			select x
		).ToArray();
		xs = xs.Del(xsNever);

		var xsUptodate = (
			from x in xs
			where released.ContainsKey(x)
			let releasedVer = released[x]
			where releasedVer == sln.Version
			select x
		).ToArray();
		xs = xs.Del(xsUptodate);

		var xsReady = (
			from x in xs
			where !cached.ContainsKey(x)
			where released.ContainsKey(x)
			let releasedVer = released[x]
			where releasedVer < sln.Version
			select x
		).ToArray();
		xs = xs.Del(xsReady);


		KeyValuePair<string, PrjReleaseNfo>[] GetNfo(IEnumerable<string> items, PrjStatus status)
			=> items.SelectA(x => new KeyValuePair<string, PrjReleaseNfo>(
				x,
				x.GetPrjNfo(status, released, cached)
			));


		var results =
			GetNfo(xsNotPackable, PrjStatus.NotPackable)
				.Concat(GetNfo(xsNever, PrjStatus.Never))
				.Concat(GetNfo(xsReady, PrjStatus.Ready))
				.Concat(GetNfo(xsPending, PrjStatus.Pending))
				.Concat(GetNfo(xsUptodate, PrjStatus.UptoDate))
				.Concat(GetNfo(xsErr, PrjStatus.ERROR))
				.ToDictionary(
					kv => kv.Key,
					kv => kv.Value
				);

		var cmpXs = sln.Prjs.Select(e => e.Name).OrderBy(e => e).ToArray();
		var cmpYs = results.Keys.OrderBy(e => e).ToArray();
		if (!cmpXs.SequenceEqual(cmpYs))
			throw new ArgumentException($"Error - inconsistent classified projects: [{string.Join(", ", cmpXs)}] vs [{string.Join(", ", cmpYs)}]");
		if (xs.Length > 0)
		{
			results.Dump();
			throw new ArgumentException($"Error - unclassified projects: [{string.Join(", ", xs)}]");
		}

		return new PrjReleaseInfos(results);
	}





	public static UserActions GetUserActions_From_FileStateAndReleaseInfos(
		SlnFileState file,
		PrjReleaseInfos release
	)
	{
		var list = new List<IUsr>();

		var isGitNotClean = file.GitStatus != GitStatus.Clean;
		var isAnyPrjError = release.Get(e => e is PrjStatus.ERROR).Length > 0;
		var isAnyPrjPending = release.Get(e => e is PrjStatus.Pending).Length > 0;
		var prjsReleasableLocally = release.Get(e => e is PrjStatus.Never or PrjStatus.Ready or PrjStatus.UptoDate);
		var prjsReleasableRemotely = release.Get(e => e is PrjStatus.Never or PrjStatus.Ready);

		if (isGitNotClean)
		{
			list.Add(new PushChangesUsr());
		}
		else if (!isAnyPrjError && !isAnyPrjPending)
		{
			list.Add(new BumpVersionUsr());

			if (prjsReleasableLocally.Length > 0)
			{
				list.Add(new ReleaseLocallyUsr(prjsReleasableLocally));
			}

			if (prjsReleasableRemotely.Length > 0)
			{
				list.Add(new ReleaseRemotelyUsr(prjsReleasableRemotely));
			}
		}

		return new UserActions([..list]);
	}





	public static Sln MakeFinal(
		SlnFileState file,
		SlnNugetState nuget,
		PrjReleaseInfos release,
		UserActions actions
	) => new(
		file.File,
		file.Version,
		file.Prjs.SelectA(prj => new Prj(
			prj.File,
			prj.IsPackable,
			prj.PrjRefs,
			prj.PkgRefs,
			release.Map[prj.Name].Status,
			release.Map[prj.Name].VersionReleased,
			release.Map[prj.Name].VersionPending
		)).ToArray(),
		file.GitStatus,
		actions
	);
	
	
	



	static PrjReleaseNfo GetPrjNfo(this string x, PrjStatus status, IReadOnlyDictionary<string, Version> released, IReadOnlyDictionary<string, Version> cached)
	{
		var verReleased = released.GetValueOrDefault(x);
		var verCached = cached.GetValueOrDefault(x);
		return new PrjReleaseNfo(
			status,
			verReleased,
			(verCached > verReleased) switch
			{
				true => verCached,
				false => null,
			}
		);
	}



	static T[] Del<T>(this T[] xs, T[] del) => xs.WhereA(e => !del.Contains(e));

	static Version? GetVersionInBoth(
		string x,
		IReadOnlyDictionary<string, Version> map1,
		IReadOnlyDictionary<string, Version> map2
	) => new[]
	{
		map1.GetValueOrDefault(x),
		map2.GetValueOrDefault(x),
	}.Max();

}