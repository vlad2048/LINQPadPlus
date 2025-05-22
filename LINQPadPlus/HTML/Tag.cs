using System.Diagnostics.CodeAnalysis;
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
	readonly ISig whenPreRender = new Sig();
	readonly ISig whenPostRender = new SigAsync();

	public IObservable<Unit> WhenPostRender => whenPostRender;

	[field: AllowNull, MaybeNull]
	public string Id
	{
		get => field ??= IdGen.Make();
		set
		{
			if (field != null) throw new InvalidOperationException("Id can only be set once. You neet to call id() before listen() or Run()");
			field = value;
		}
	}


	protected internal Tag(string name) => this.name = name;

	
	public object ToDump() => Util.RawHtml(RenderString(true));

	public override string ToString() => RenderString(false).BeautifyHtml();

	internal string RenderString(bool runJs)
	{
		if (runJs)
		{
			var readyDispatchId = $"{Id}-OnReady";
			Events.Listen(readyDispatchId, whenPostRender.Trig);
			this.Run("_ => dispatch(____0____, {})", e => e.JSRepl_Val(0, readyDispatchId));
			
			whenPreRender.Trig();
		}

		var sb = new StringBuilder();
		sb.WriteTagOpenStart(name);
		sb.WriteId(Id);
		sb.WriteClasses(classes);
		foreach (var (key, val) in attrs.Where(t => t.Value != null))
			val!.WriteAttribute(sb, key);
		sb.WriteStyles(styles);
		sb.WriteTagOpenEnd();
		
		foreach (var kid in kids)
		{
			var kidStr = kid.RenderString(runJs);
			sb.Append(kidStr);
		}

		if (!t.VoidElements.Contains(name))
			sb.WriteTagClose(name);
		
		return sb.ToString();
	}

	// @formatter:off
	public Tag id(string id__)											=> this.With(() => Id = id__);
	public Tag cls(string className, bool condition = true)				=> this.With(() => classes.Add(className), condition);
	public Tag style(string style, bool condition = true)				=> this.With(() => styles.Add(style), condition);
	public Tag attr(string key, JSVal? value, bool condition = true)	=> this.With(() => attrs[key] = value, condition);
	// @formatter:on

	public Tag this[params HtmlNode[] kids_] => this.With(() =>
	{
		if (t.VoidElements.Contains(name))
			throw new ArgumentException($"<{name}> elements cannot have children");
		kids.AddRange(kids_);
	});

	public Tag js(
		string onRender,
		[CallerMemberName] string? srcMember = null,
		[CallerFilePath] string? srcFile = null,
		[CallerLineNumber] int srcLine = 0
	) =>
	// ReSharper disable ExplicitCallerInfoArgument
		this.With(() => whenPostRender.Subscribe(_ => this.Run(onRender, null, srcMember, srcFile, srcLine)));
	// ReSharper restore ExplicitCallerInfoArgument
	
	
	public Tag on<T>(IRoVar<T> Δrx, Func<Tag, T, Tag> preRender, string postRender) =>
		this.With(() =>
		{
			whenPreRender.Subscribe(_ => preRender(this, Δrx.V));
			Obs.CombineLatest(whenPostRender, Δrx, (_, state) => state)
				.Subscribe(state => this.Run(
					"""
					function (elt) {
						(____0____)(elt, ____1____);
					}
					""",
					e => e
						.JSRepl_Obj(0, postRender)
						.JSRepl_Val(1, JSVal.Make(state))
				), Δrx.CancelToken);
		});


	public Tag onReady(Action onReady) => this.With(() => whenPostRender.Subscribe(_ => onReady()));
	
	
	public Tag listen(string eventName, string? js, Action<string> action, bool condition = true) =>
		this.With(() =>
		{
			var dispatchId = $"{Id}-{eventName}";

			whenPreRender.Subscribe(_ =>
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

			whenPostRender.Subscribe(_ =>
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
				try {
					const elt = await window.waitForElement(____998____);
					(____999____)(elt);
				} catch (err) {
					dispatchError(err, runId);
				}
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
		var stylesStr = styles.Select(e => e.Trim().RemoveSuffixIFN(";")).JoinText("; ");
		sb.Append($" style='{stylesStr}'");
	}
	public static void WriteTagOpenEnd(this StringBuilder sb) => sb.Append(">");

	public static void WriteTagClose(this StringBuilder sb, string name) => sb.Append($"</{name}>");

	static string RemoveSuffixIFN(this string s, string suffix) =>
		s.EndsWith(suffix) switch
		{
			true => s[..^suffix.Length],
			false => s,
		};
}