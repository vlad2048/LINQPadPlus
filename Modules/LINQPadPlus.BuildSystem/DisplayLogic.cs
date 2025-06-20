using System.Reactive.Linq;
using LINQPad;
using LINQPadPlus.BuildSystem._sys;
using LINQPadPlus.BuildSystem._sys.Structs;
using LINQPadPlus.Rx;

namespace LINQPadPlus.BuildSystem;


static class DisplayLogicTester
{
	static readonly Sln slnTest =
		new(
			"LINQPadPlus",
			new Version(0, 0, 10),
			[
				new Prj("LINQPadPlus", false, [], [], PrjStatus.NotPackable, null, null),
				new Prj("LINQPadPlus.Plotly", true, [], [], PrjStatus.Ready, null, null),
				new Prj("LINQPadPlus.Tabulator", true, [], [], PrjStatus.Ready, new Version(0, 0, 9), null),
				new Prj("LINQPadPlus.BuildSystem", true, [], [], PrjStatus.Pending, new Version(0, 0, 9), new Version(0, 0, 10)),
				new Prj("LINQPadPlus.Other", true, [], [], PrjStatus.UptoDate, new Version(0, 0, 10), null),
				new Prj("LINQPadPlus.Milou", true, [], [], PrjStatus.ERROR, new Version(0, 0, 16), null),
			],
			GitStatus.UnStaged
		);

	public static void Run()
	{
		var dcLog = new DumpContainer();
		var ΔcommitMsg = Var.Make("").D();
		var Δstatus = Var.Make(GitStatus.Clean).D();
		var exec = new TestExec();
		t.Div[[
			Enum.GetValues<GitStatus>().Select(e => t.Button[$"{e}"].click(() => Δstatus.V = e)).ToArray(),
			Δstatus
				.Select(status => slnTest with { GitStatus = status })
				.Select(sln => sln.Display(ΔcommitMsg, exec)).ToDC(),
			dcLog,
		]].Dump();
	}
}



static class Ctrls
{
	public static Tag LinkButton(string name, Action action) =>
		t.A[name]
			.click(() => Task.Run(action));

	public static Tag TextBox(IRwVar<string> Δrx, string placeholder) =>
		t.Input
			.attr("placeholder", placeholder)
			.on(Δrx, (t, v) => t.attr("value", v), "(elt, v) => elt.value = v")
			.listen("input", "e => e.value", v => Δrx.V = v);
}



static class DisplayLogic
{
	public static void InitCss() =>
		Util.HtmlHead.AddStyles(
			"""
			 .noheaders .typeheader { display: none; }
			""");

	public static object Display(this Sln sln, IRwVar<string> ΔcommitMsg, IExec exec) =>
		t.Div.cls("noheaders")[[
			new DumpContainer(
				new
				{
					sln.Name,
					Release = sln.IsReleasable(out var reason) switch
					{
						false => t.Span[reason!].style($"color:{C.TextRed}"),
						true => Ctrls.LinkButton("release", () => exec.Release(sln)),
					},
					//Release = Ctrls.LinkButton("release", () => exec.Release(sln)),
					Version = sln.DisplayVersion(ΔcommitMsg, exec),
					Git = sln.DisplayGitStatus(ΔcommitMsg, exec),
					Projects = sln.Prjs.Select(prj => prj.Display(exec)),
				}
			)
		]];

	static Tag DisplayVersion(this Sln sln, IRwVar<string> ΔcommitMsg, IExec exec) =>
		DivHorzSpaced[[
			t.Span[$"{sln.Version}"],
			Ctrls.LinkButton("bump", () =>
			{
				var wasClean = sln.GitStatus is GitStatus.Clean;
				var versionBump = exec.BumpVersion(sln);
				if (wasClean)
					ΔcommitMsg.V = $"v{versionBump}";
			}),
		]];

	static Tag DisplayGitStatus(this Sln sln, IRwVar<string> ΔcommitMsg, IExec exec) =>
		DivHorzSpaced[[
			t.Span[$"{sln.GitStatus}"].style($"color:{(sln.GitStatus is GitStatus.Clean ? C.TextGreen : C.TextRed)}"),
			DivHorz[[
				Ctrls.TextBox(ΔcommitMsg, "commit message")
					.If(sln.GitStatus is GitStatus.UnStaged or GitStatus.UnCommited),
				Ctrls.LinkButton("push", () => exec.Push(sln, ΔcommitMsg.V))
					.If(sln.GitStatus is GitStatus.UnStaged or GitStatus.UnCommited or GitStatus.UnPushed),
			]],
		]];

	static object Display(this Prj e, IExec exec) => new
	{
		//e.Name,
		Name = DivVert[[
			e.Name,
			Ctrls.LinkButton("release locally", () => exec.ReleasePrjLocally(e))
				.If(e.IsPackable),
		]],
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
			.style($"color:{C.StatusColors[e.Status]}")
			[[
				C.StatusText[e.Status],
			]];

	static HtmlNode ShowPending(this Prj e) =>
		t.Span
			.style(cssPending)
			.style($"color:{C.StatusColors[e.Status]}")
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

	static Tag DivVert => t.Div.style("display:flex; flex-direction:column");
	static Tag DivHorzSpaced => t.Div.style("display:flex; align-items:baseline; justify-content:space-between");
	static Tag DivHorz => t.Div.style("display:flex; align-items:baseline; column-gap:10px");

	const string cssWrapStatus = "";
	const string cssWrapNuget = "";
	const string cssStatus = "";
	const string cssPending = "";
	const string cssReleased = "color:#ff43d5; font-weight:bold";
}




file static class C
{
	public const string TextRed = "#f15858";
	public const string TextGreen = "#4beb52";

	// @formatter:off
	public static readonly Dictionary<PrjStatus, string> StatusText = new()
	{
		{ PrjStatus.NotPackable,    "Not packable" },
		//{ PrjStatus.Never,          "Never released" },
		{ PrjStatus.Ready,          "Ready for release" },
		{ PrjStatus.Pending,        "Release pending" },
		{ PrjStatus.UptoDate,       "Upto date" },
		{ PrjStatus.ERROR,          "ERROR" },
	};
	public static readonly Dictionary<PrjStatus, string> StatusColors = new()
	{
		{ PrjStatus.NotPackable,    "#818181" },
		//{ PrjStatus.Never,          "#ebebeb" },
		{ PrjStatus.Ready,          "#2bccff" },
		{ PrjStatus.Pending,        "#fd7d2f" },
		{ PrjStatus.UptoDate,       "#36dd68" },
		{ PrjStatus.ERROR,          "#f53e1e" },
	};
	// @formatter:on
}

