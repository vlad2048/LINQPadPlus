using LINQPad;
using LINQPad.Controls;
using LINQPadPlus._sys;

namespace LINQPadPlus;

public static class Events
{
	const string DispatcherDivId = "dispatcher";
	static Div? dispatcherDiv;
	static Div DispatcherDiv => dispatcherDiv ?? throw new ArgumentException("Events.Init() was not called");

	public const string ErrorDispatcherId = nameof(ErrorDispatcherId);

	internal static void Init()
	{
		dispatcherDiv = new Div { HtmlElement = { ID = DispatcherDivId } }.Dump();
		JS.Run(
			"""
			____0____ = document.getElementById('____0____');
			function dispatch(dispatchId, evtArg) {
				____0____.dispatchEvent(new CustomEvent(
					dispatchId,
					{
						detail: evtArg
					}
				));
			}
			""",
			e => e
				.JSRepl_Var(0, DispatcherDivId)
		);
		
		ListenWithArg(ErrorDispatcherId, JSRunLogic.ErrorReceived);
	}
	
	
	/// <summary>
	/// Warning
	/// Much slower than Listen() for some reason
	/// </summary>
	public static void ListenWithArg(string dispatchId, Action<string> action)
	{
		static string ReadProp(PropertyEventArgs args)
		{
			if (!args.Properties.TryGetValue("detail", out var str)) throw new ArgumentException("(Impossible) Detail property not found in custom event");
			return str;
		}
		
		DispatcherDiv.HtmlElement.AddEventListener(dispatchId, ["detail"], (_, e) =>
		{
			action(ReadProp(e));
		});
		Thread.Sleep(10);
	}



	public static void Listen(string dispatchId, Action action)
	{
		DispatcherDiv.HtmlElement.AddEventListener(dispatchId, (_, _) =>
		{
			action();
		});

		/*
		***********
		* WARNING *
		***********
		
		Without this delay we can miss the event sometimes.
	
		Example:
		========
			void Main()
			{
				var Δon = Var.Make(true);

				var btn = t.Button["Btn"].enable(Δon);
				var btnToggle = t.Button["toggle"].listen("click", () =>
				{
					Δon.V = !Δon.V;
				});
				
				btnToggle.p();
				btn.p();
			}
		
		
		Also, add these in the Tag constructor:
		=======================================
			sigPreRender.Subscribe(_ => $"[{kids[0].Text}] OnSigPreRender".Dump());
			sigPostRender.Subscribe(_ => $"[{kids[0].Text}] OnSigPostRender".Dump());
		
		*/
		Thread.Sleep(10);
	}
}