using LINQPadPlus.Rx;

namespace LINQPadPlus._sys.TagUtils;

sealed record Mutator(
	RoVar<JSVal> State,
	Action<JSVal> SetOffline,
	string SetOnline
)
{
	public static Mutator Make(RoVar<int> state, Action<int> setOffline, string setOnline) =>
		new(
			state.SelectVar(e => (JSVal)e),
			e => setOffline(e.I!.Value),
			setOnline
		);

	public static Mutator Make(RoVar<double> state, Action<double> setOffline, string setOnline) =>
		new(
			state.SelectVar(e => (JSVal)e),
			e => setOffline(e.D!.Value),
			setOnline
		);

	public static Mutator Make(RoVar<bool> state, Action<bool> setOffline, string setOnline) =>
		new(
			state.SelectVar(e => (JSVal)e),
			e => setOffline(e.B!.Value),
			setOnline
		);

	public static Mutator Make(RoVar<string> state, Action<string> setOffline, string setOnline) =>
		new(
			state.SelectVar(e => (JSVal)e),
			e => setOffline(e.S!),
			setOnline
		);
}