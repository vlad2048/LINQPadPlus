using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LINQPad;
using LINQPad.Controls;
using LINQPadPlus.BuildSystem._sys;
using LINQPadPlus.BuildSystem._sys.CsProjLogic;
using LINQPadPlus.BuildSystem._sys.NugetLogic;
using LINQPadPlus.BuildSystem._sys.Structs;
using LINQPadPlus.BuildSystem._sys.Utils;
using LINQPadPlus.Rx;

namespace LINQPadPlus.BuildSystem;


public static class NugetManager
{
	public static async Task Run(string slnFile, string nugetUrl, string nugetKey)
	{
		try
		{
			await RunInternal(slnFile, nugetUrl, nugetKey);
		}
		catch (Exception ex)
		{
			ex.Dump();
		}
	}
	
	static async Task RunInternal(string slnFile, string nugetUrl, string nugetKey)
	{
		DisplayLogic.InitCss();
		var dcLog = new DumpContainer();
		var dcImportant = new DumpContainer();

		var ΔwhenRefresh = new Subject<Unit>().D();
		var Δerr = Var.Make<string?>(null).D();
		var Δfile = Var.Make(await SlnLoader.Load(slnFile, dcLog)).D();
		var Δnuget = Var.Make(NugetAPI.GetState(Δfile.V, nugetUrl)).D();
		var Δrelease = Var.Expr(() => SlnStateLogic.GetReleaseInfos_From_FileAndNugetState(Δfile.V, Δnuget.V));
		var Δsln = Var.Expr(() => SlnStateLogic.MakeFinal(Δfile.V, Δnuget.V, Δrelease.V));

		WatchFiles(Δfile, Δerr, ΔwhenRefresh, slnFile, dcLog, dcImportant).D();
		PollNuget(Δfile, Δnuget, ΔwhenRefresh, nugetUrl).D();

		var ΔcommitMsg = Var.Make("").D();
		var exec = new Exec(dcLog, nugetUrl, nugetKey, () => ΔwhenRefresh.OnNext(Unit.Default));

		// Display
		// =======
		t.Div[[
			dcImportant,
			t.Div.style("display:flex; column-gap:10px")[[
				Δsln
					.DistinctUntilChanged(Sln.EqualityComparer)
					.CombineLatest(Δerr, (sln, err) => (sln, err))
					.Select(e => e.err switch
					{
						not null => t.Span[e.err].style("color:#f00"),
						null => e.sln.Display(ΔcommitMsg, exec),
					})
					.ToDC(),
				dcLog,
			]],
		]].Dump();
	}
	

	static IDisposable WatchFiles(
		IRwVar<SlnFileState> Δfile,
		IRwVar<string?> Δerr,
		IObservable<Unit> ΔwhenRefresh,
		string slnFile,
		DumpContainer dc,
		DumpContainer dcImportant
	) =>
		RxFolderWatcher.Watch(Path.GetDirectoryName(slnFile)!)
			.Where(e => !Δfile.V.IgnoreFolders.Any(e.StartsWith))
			.Throttle(Consts.FileDebouncePeriod)
			.ToUnit()
			.Merge(ΔwhenRefresh)
			.Select(_ => Obs.FromAsync(async () => await SlnLoader.Load(slnFile, dc))
				.Retry(4)
				.Materialize()
			)
			.Switch()
			.Subscribe(
				e =>
				{
					switch (e.Kind)
					{
						case NotificationKind.OnNext:
							Δerr.V = null;
							Δfile.V = e.Value;
							break;
						case NotificationKind.OnError:
							Δerr.V = e.Exception!.Message;
							dcImportant.AppendContent(e.Exception!.Message);
							break;
					}
				}
			);


	static IDisposable PollNuget(
		IRoVar<SlnFileState> Δfile,
		IRwVar<SlnNugetState> Δnuget,
		IObservable<Unit> ΔwhenRefresh,
		string nugetUrl
	) =>
		Obs.Merge(
				Obs.Interval(Consts.NugetDebouncePeriod).ToUnit().Prepend(Unit.Default),
				ΔwhenRefresh
			)
			.WithLatestFrom(Δfile, (_, file) => file)
			.Subscribe(file => Δnuget.V = NugetAPI.GetState(file, nugetUrl));
	
	/*Δrelease
		.CombineLatest(Δfile, ΔwhenRefresh, (release, file, _) => (release, file))
		.Select(e => e.release.NeedsNugetPolling(e.file))
		.DistinctUntilChanged()
		.Select(e => e switch
		{
			false => Obs.Never<Unit>(),
			true => Obs.Interval(Consts.NugetDebouncePeriod).ToUnit().Prepend(Unit.Default),
		})
		.Switch()
		.CombineLatest(ΔwhenRefresh, (e, _) => e)
		.WithLatestFrom(Δfile, (_, file) => file)
		.Subscribe(file => Δnuget.V = NugetAPI.GetState(file, nugetUrl));*/
}