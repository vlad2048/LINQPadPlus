using LINQPadPlus._sys;
using LINQPadPlus.Rx;

namespace LINQPadPlus;

public static partial class Ctrls
{
	public static Tag ToTable<T>(this RoVar<T[]> Δitems, TableOptions<T>? opts = null) => TableLogic.Make(Δitems, opts, null);
	public static Tag ToTable<T>(this IEnumerable<T> items, TableOptions<T>? opts = null) => ((RoVar<T[]>)items.ToArray()).ToTable(opts);

	public static (RoVar<T>, Tag) ToTableSelector<T>(this RoVar<T[]> Δitems, TableOptions<T> opts)
	{
		var Δrx = Var.Make(Δitems.V[0]);
		var tag = TableLogic.Make(Δitems, opts, idx => Δrx.V = Δitems.V[idx]);
		return (Δrx, tag);
	}
	public static (RoVar<T>, Tag) ToTableSelector<T>(this IEnumerable<T> items, TableOptions<T> opts) => ((RoVar<T[]>)items.ToArray()).ToTableSelector(opts);

	public static Tag ToTableSelector<T>(this RoVar<T[]> Δitems, RwVar<T> Δrx, TableOptions<T> opts) => TableLogic.Make(Δitems, opts, idx => Δrx.V = Δitems.V[idx]);
	public static Tag ToTableSelector<T>(this IEnumerable<T> items, RwVar<T> Δrx, TableOptions<T> opts) => ((RoVar<T[]>)items.ToArray()).ToTableSelector(Δrx, opts);
}
