using System.Reactive;
using System.Reactive.Linq;
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
	public static async Task Run(string slnFile, string nugetApiKey)
	{
		try
		{
			await RunInternal(slnFile, nugetApiKey);
		}
		catch (Exception ex)
		{
			ex.Dump();
		}
	}
	
	static async Task RunInternal(string slnFile, string nugetApiKey)
	{
		DisplayLogic.InitCss();
		var dcLog = new DumpContainer();
		var dcImportant = new DumpContainer();

		var Δerr = Var.Make<string?>(null).D();
		var Δfile = Var.Make(await SlnLoader.Load(slnFile, dcLog)).D();
		var Δnuget = Var.Make(NugetAPI.GetState(Δfile.V)).D();
		var Δrelease = Var.Expr(() => SlnStateLogic.GetReleaseInfos_From_FileAndNugetState(Δfile.V, Δnuget.V));
		var Δsln = Var.Expr(() => SlnStateLogic.MakeFinal(Δfile.V, Δnuget.V, Δrelease.V));

		WatchFiles(Δfile, Δerr, slnFile, dcLog, dcImportant).D();
		PollNuget(Δrelease, Δfile, Δnuget).D();

		var ΔcommitMsg = Var.Make("").D();
		var exec = new Exec(dcLog, nugetApiKey);

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
	

	static IDisposable WatchFiles(IRwVar<SlnFileState> Δfile, IRwVar<string?> Δerr, string slnFile, DumpContainer dc, DumpContainer dcImportant) =>
		RxFolderWatcher.Watch(Path.GetDirectoryName(slnFile)!)
			.Where(e => !Δfile.V.IgnoreFolders.Any(e.StartsWith))
			.Throttle(Consts.FileDebouncePeriod)
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
		IRoVar<PrjReleaseInfos> Δrelease,
		IRoVar<SlnFileState> Δfile,
		IRwVar<SlnNugetState> Δnuget
	) =>
		Δrelease
			.CombineLatest(Δfile, (release, file) => (release, file))
			.Select(e => e.release.NeedsNugetPolling(e.file))
			.DistinctUntilChanged()
			.Select(e => e switch
			{
				false => Obs.Never<Unit>(),
				true => Obs.Interval(Consts.NugetDebouncePeriod).ToUnit().Prepend(Unit.Default),
			})
			.Switch()
			.WithLatestFrom(Δfile, (_, file) => file)
			.Subscribe(file => Δnuget.V = NugetAPI.GetState(file));
}