using System.Reactive.Subjects;
using System.Reactive;
using System.Text;
using LINQPad;
using LINQPadPlus._sys;
using LINQPadPlus._sys.Utils;
using LINQPadPlus.Rx;
using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using LINQPadPlus._sys.TagUtils;

namespace LINQPadPlus;



public class Tag
{
	readonly string name;
	readonly Dictionary<string, JSVal?> attrs = new();
	readonly HashSet<string> classes = [];
	readonly List<string> styles = [];
	readonly List<HtmlNode> kids = [];
	readonly ISig sigPreRender = new Sig();
	readonly ISig sigPostRender = new SigAsync();

	public string Id { get; } = IdGen.Make();

	internal Tag(string name)
	{
		this.name = name;

		var readyDispatchId = $"{Id}-OnReady";
		sigPreRender.Subscribe(_ => Events.Listen(readyDispatchId, sigPostRender.Trig));
		this.Run("_ => dispatch(____0____, {})", e => e.JSRepl_Val(0, readyDispatchId));
	}

	
	public object ToDump() => Util.RawHtml(RenderString(true));

	public override string ToString() => RenderString(false);

	internal string RenderString(bool runJs)
	{
		if (runJs)
			sigPreRender.Trig();

		var sb = new StringBuilder();
		sb.WriteTagOpenStart(name);
		sb.WriteId(Id);
		sb.WriteClasses(classes);
		sb.WriteStyles(styles);
		foreach (var (key, val) in attrs.Where(t => t.Value != null))
			val!.WriteAttribute(sb, key);
		sb.WriteTagOpenEnd();
		
		foreach (var kid in kids)
		{
			var kidStr = kid.RenderString(runJs);
			sb.Append(kidStr);
		}

		sb.WriteTagClose(name);
		return sb.ToString();
	}

	// @formatter:off
	public Tag this[params HtmlNode[] kids_]							=> this.With(() => kids.AddRange(kids_));
	public Tag cls(string className, bool condition = true)				=> this.With(() => classes.Add(className), condition);
	public Tag style(string style, bool condition = true)				=> this.With(() => styles.Add(style), condition);
	public Tag style(string[] styles_, bool condition = true)			=> this.With(() => styles.AddRange(styles_), condition);
	public Tag attr(string key, JSVal? value, bool condition = true)	=> this.With(() => attrs[key] = value, condition);
	public Tag js(string onRender)										=> this.With(() => sigPostRender.Subscribe(_ => this.Run(onRender)));
	// @formatter:on




	public Tag enable(RoVar<bool> Δon) => AddMutator(Mutator.Make(
		Δon,
		on => attrs["disabled"] = $"{!on}",
		"(elt, on) => elt.disabled = !on"
	));

	
	public Tag onReady(Action onReady) => this.With(() => sigPostRender.Subscribe(_ => onReady()));
	
	
	public Tag listen(string eventName, string? js, Action<string> action, bool condition = true) =>
		this.With(() =>
		{
			var dispatchId = $"{Id}-{eventName}";

			sigPreRender.Subscribe(_ =>
				Events.Listen(dispatchId, () =>
				{
					var args = js switch
					{
						not null =>
							JS.Return(
								"""
								(function () {
									const elt = document.getElementById(____0____);
									return (____1____)(elt);
								})()
								""",
								e => e
									.JSRepl_Val(0, Id)
									.JSRepl_Obj(1, js)
							),
						null => string.Empty,
					};
					action(args);
				})
			);

			sigPostRender.Subscribe(_ =>
				this.Run(
					"""
					function (elt) {
						elt.addEventListener(____0____, evt => {
							dispatch(____1____, {});
						});
					}
					""",
					e => e
						.JSRepl_Val(0, eventName)
						.JSRepl_Val(1, dispatchId)
				)
			);
		}, condition);





	Tag AddMutator(Mutator mutator) => this.With(() =>
	{
		sigPreRender.Subscribe(_ => mutator.SetOffline(mutator.State.V));
		Obs.CombineLatest(sigPostRender, mutator.State, (_, state) => state)
			.Subscribe(state => this.Run(
				"""
				function (elt) {
					(____0____)(elt, ____1____);
				}
				""",
				e => e
					.JSRepl_Obj(0, mutator.SetOnline)
					.JSRepl_Val(1, state)
			)).D(D);
	});
}




public static class TagUtils
{
	public static Tag listen(this Tag tag, string eventName, Action action, bool condition = true) => tag.listen(eventName, null, _ => action(), condition);


	public static void Run(
		this Tag tag,
		[LanguageInjection(InjectedLanguage.JAVASCRIPT)] string codeFun,
		Func<string, string>? replFun = null,
		[CallerMemberName] string? srcMember = null,
		[CallerFilePath] string? srcFile = null,
		[CallerLineNumber] int srcLine = 0
	) =>
		JS.Run(
			"""
			(async () => {
				const elt = await window.waitForElement(____998____);
				(____999____)(elt);
			})();
			""",
			e =>
				JS.Fmt(
					e
						.JSRepl_Val(998, tag.Id)
						.JSRepl_Var(999, codeFun),
					replFun
				),
			srcMember,
			// ReSharper disable ExplicitCallerInfoArgument
			srcFile,
			srcLine
			// ReSharper restore ExplicitCallerInfoArgument
		);
}

/*
static class TagFileUtils
{
	public static string JSRepl_Val<T>(this string c, int i, T x)
	{
		if (typeof(T) == typeof(string))
			return c.JSRepl_Val(i, x as string);
		if (typeof(T) == typeof(bool))
			return c.JSRepl_Val(i, (bool)(x as object)!);
		if (typeof(T) == typeof(double))
			return c.JSRepl_Val(i, (double)(x as object)!);
		if (typeof(T) == typeof(int))
			return c.JSRepl_Val(i, (int)(x as object)!);
		throw new ArgumentException($"TagFileUtils.JSRepl_Val<T> does not support type: {typeof(T).Name}");
	}
}
*/








file static class TagWriter
{
	public static void WriteTagOpenStart(this StringBuilder sb, string name) => sb.Append($"<{name}");

	public static void WriteId(this StringBuilder sb, string id) => sb.Append($" id='{id}'");
	
	public static void WriteClasses(this StringBuilder sb, HashSet<string> classes)
	{
		if (classes.Count == 0) return;
		sb.Append($" class='{classes.JoinText(" ")}'");
	}
	public static void WriteStyles(this StringBuilder sb, List<string> styles)
	{
		if (styles.Count == 0) return;
		sb.Append($" style='{styles.JoinText("; ")}'");
	}
	public static void WriteTagOpenEnd(this StringBuilder sb) => sb.Append(">");

	public static void WriteTagClose(this StringBuilder sb, string name) => sb.Append($"</{name}>");
}