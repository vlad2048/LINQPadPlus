using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using LINQPad;
using LINQPadPlus.BuildSystem._sys;
using LINQPadPlus.BuildSystem._sys.CsProjLogic;
using LINQPadPlus.BuildSystem._sys.NugetLogic;
using LINQPadPlus.BuildSystem._sys.Structs;
using LINQPadPlus.BuildSystem._sys.Utils;
using LINQPadPlus.Rx;

namespace LINQPadPlus.BuildSystem;

public static class NugetManager
{
	public static void Run(string slnFile)
	{
		InitCss();
		var dcLog = new DumpContainer();
		var dcMain = new DumpContainer();

		var ΔslnFile = RxFolderWatcher.Watch(Path.GetDirectoryName(slnFile)!, Consts.FileDebouncePeriod)
			.Prepend(slnFile)
			.Do(e => dcLog.Log($"Refresh Git: {e}"))
			.Select(_ => SlnLoader.Load(slnFile, dcLog))
			.Publish();


		var ΔnugetRefresh = new Subject<Unit>().D();

		var Δnuget = ΔnugetRefresh
			.Do(_ => dcLog.Log("Refresh Nuget 0"))
			.WithLatestFrom(ΔslnFile, (_, file) => file)
			.Do(_ => dcLog.Log("Refresh Nuget 1"))
			.Sample(Consts.NugetDebouncePeriod)
			.Do(_ => dcLog.Log("Refresh Nuget 2"))
			.Select(NugetAPI.GetState)
			.Do(_ => dcLog.Log("Refresh Nuget 3"))
			.Publish();

		var Δrelease =
			Obs.CombineLatest(
					ΔslnFile,
					Δnuget,
					(file, nuget) => (file, nuget)
				)
				.Do(_ => dcLog.Log("Refresh Release"))
				.Select(e => SlnStateLogic.GetReleaseInfos_From_FileAndNugetState(e.file, e.nuget))
				.Publish();

		Δrelease
			//.Where(e => e.NeedsNugetPolling)
			//.Prepend(null)
			.Do(_ => dcLog.Log("Refresh Release_2"))
			.Subscribe(_ => ΔnugetRefresh.OnNext(Unit.Default)).D();
		
		var Δactions =
			Obs.CombineLatest(
					ΔslnFile,
					Δrelease,
					(file, release) => (file, release)
				)
				.Do(_ => dcLog.Log("Refresh Actions"))
				.Select(e => SlnStateLogic.GetUserActions_From_FileStateAndReleaseInfos(e.file, e.release));



		//var btnClear = t.Button["Clear log"].listen("click", () => dcLog.LogClear());
		//var btnBump = t.Button["Bump version"].listen("click", () => { });
		//var btnLocal = t.Button["Release locally"].listen("click", () => { });
		//var btnRemote = t.Button["Release remotely"].listen("click", () => { });
		var btnRefresh = t.Button["Refresh nuget"].listen("click", () => ΔnugetRefresh.OnNext(Unit.Default));


		// Update the UI
		// =============
		Obs.CombineLatest(
				ΔslnFile,
				Δnuget,
				Δrelease,
				Δactions,
				(file, nuget, release, actions) => (file, nuget, release, actions)
			)
			.Do(_ => dcLog.Log("Refresh UI"))
			.Select(e => SlnStateLogic.MakeFinal(e.file, e.nuget, e.release, e.actions))
			.DistinctUntilChanged(Sln.EqualityComparer)
			.Subscribe(sln => dcMain.UpdateContent(sln.Display())).D();


		// Render the UI
		// =============
		t.Div[[
			t.Div.style("display:flex; column-gap:10px")[[
				//btnClear,
				//btnBump,
				//btnLocal,
				//btnRemote,
				btnRefresh
			]],
			dcMain,
			dcLog,
		]].Dump();

		
		
		ΔslnFile.Connect().D();
		Δnuget.Connect().D();
		Δrelease.Connect().D();
	}


	/*static IObservable<T> MakeHot<T>(this IObservable<T> source)
	{
		var hot = source.Publish();
		hot.Connect();
		return hot;
	}*/


	/*
	public static void Run(string slnFile)
	{
		InitCss();
		
		var dcLog = new DumpContainer();
		var dcMain = new DumpContainer();

		var subj = new Subject<Sln>();
		Sln? slnPrev = null;
		
		void Refresh(bool nugetRefresh)
		{
			dcLog.Log($"Refresh (nuget: {nugetRefresh})");
			subj.OnNext(Sln.Load(
				slnFile,
				nugetRefresh ? null : slnPrev?.ReleasedMap,
				dcLog
			));
		}
		
		var btnClear = t.Button["Clear log"].listen("click", () => dcLog.LogClear());
		var btnBump = t.Button["Bump version"].listen("click", () => { });
		var btnLocal = t.Button["Release locally"].listen("click", () => { });
		var btnRemote = t.Button["Release remotely"].listen("click", () => { });


		// Update the UI
		// =============
		subj
			.DistinctUntilChanged(Sln.EqualityComparer)
			.Subscribe(sln => dcMain.UpdateContent(sln.Display())).D();
		

		// Render the UI
		// =============
		t.Div[[
			t.Div.style("display:flex; column-gap:10px")[[
				btnClear,
				btnBump,
				btnLocal,
				btnRemote,
			]],
			dcMain,
			dcLog,
		]].Dump();


		// Poll
		// ====
		Obs.Interval(Consts.NugetPollingPeriod)
			.WithLatestFrom(subj, (_, sln) => sln)
			.Subscribe(sln => Refresh(sln.NeedsNugetPolling)).D();
		
		
		// Poll Nuget if needed
		// ====================
		subj
			.Select(sln => sln.NeedsNugetPolling switch
			{
				false => Obs.Never<Unit>(),
				true => Obs.Interval(Consts.NugetPollingPeriod).ToUnit(),
			})
			.Switch()
			.Subscribe(_ =>
			{
				dcLog.Log("Polling Nuget");
				Refresh(true);
			}).D();
		

		// Poll Git
		// ========
		Obs.Interval(Consts.GitPollingPeriod)
			.Subscribe(_ =>
			{
				dcLog.Log("Polling Git");
				Refresh(false);
			}).D();
		
		
		// Initial refresh
		// ===============
		Refresh(true);
	}*/



	static object Display(this Sln sln) => new
	{
		sln.Name,
		sln.Version,
		sln.GitStatus,
		Projects = sln.Prjs.Select(prj => prj.Display()),
	};
	
	static object Display(this Prj e) => new
	{
		e.Name,
		ReleaseStatus =
			t.Div.style(cssWrapStatus)[[
				e.ShowStatus(),
				e.ShowPending(),
			]],
		NugetVersion =
			t.Div.style(cssWrapNuget)[[
				e.ShowReleased(),
			]],
		e.PrjRefs,
		e.PkgRefs,
	};

	static HtmlNode ShowStatus(this Prj e) =>
		t.Span
			.style(cssStatus)
			.style($"color:{statusColors[e.Status]}")
			[[
				statusText[e.Status],
			]];

	static HtmlNode ShowPending(this Prj e) =>
		t.Span
			.style(cssPending)
			.style($"color:{statusColors[e.Status]}")
			[[
				$" ({e.VersionPending})",
			]]
			.If(e.VersionPending != null);

	static HtmlNode ShowReleased(this Prj e) =>
		t.Span
			.style(cssReleased)
			[[
				$"{e.VersionReleased}",
			]]
			.If(e.VersionReleased != null);

	const string cssWrapStatus = "";
	const string cssWrapNuget = "";
	const string cssStatus = "";
	const string cssPending = "";
	const string cssReleased = "color:#ff43d5; font-weight:bold";


	static readonly Dictionary<PrjStatus, string> statusText = new()
	{
		{ PrjStatus.NotPackable,    "Not packable" },
		{ PrjStatus.Never,          "Never released" },
		{ PrjStatus.Ready,          "Ready for release" },
		{ PrjStatus.Pending,        "Release pending" },
		{ PrjStatus.UptoDate,       "Upto date" },
		{ PrjStatus.ERROR,          "ERROR" },
	};
	static readonly Dictionary<PrjStatus, string> statusColors = new()
	{
		{ PrjStatus.NotPackable,    "#818181" },
		{ PrjStatus.Never,          "#ebebeb" },
		{ PrjStatus.Ready,          "#2bccff" },
		{ PrjStatus.Pending,        "#fd7d2f" },
		{ PrjStatus.UptoDate,       "#36dd68" },
		{ PrjStatus.ERROR,          "#f53e1e" },
	};



	static void InitCss()
	{
		Util.HtmlHead.AddStyles(
			"""
				.typeheader {
					display: none;
				}
			""");

		/*Util.HtmlHead.AddCssLink("https://cdn.jsdelivr.net/npm/@picocss/pico@2/css/pico.min.css");
		JS.Run("""document.documentElement.setAttribute("data-theme", "dark");""");
		Util.HtmlHead.AddStyles(
			"""
				:root {
					--pico-spacing: 0rem;
					--pico-form-element-spacing-vertical: 0.375rem;
					--pico-form-element-spacing-horizontal: 0.5rem;
				}
			""");*/
	}
}