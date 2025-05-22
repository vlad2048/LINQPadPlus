using LINQPadPlus.Rx;
using LINQPadPlus.Tabulator._sys;

namespace LINQPadPlus.Tabulator;

public static class TableExtensions
{
	public static Tag ToTable<T>(this IRoVar<T[]> Δitems, TableOptions<T>? opts = null) => TableLogic.Make(Δitems, opts, null);
	public static Tag ToTable<T>(this IEnumerable<T> items, TableOptions<T>? opts = null) => Var.Make(items.ToArray()).ToTable(opts);

	public static (IRoVar<T>, Tag) ToTableSelector<T>(this IRoVar<T[]> Δitems, TableOptions<T> opts)
	{
		var Δrx = Var.Make(Δitems.V[0]);
		var tag = TableLogic.Make(Δitems, opts, idx => Δrx.V = Δitems.V[idx]);
		return (Δrx, tag);
	}
	public static (IRoVar<T>, Tag) ToTableSelector<T>(this IEnumerable<T> items, TableOptions<T> opts) => Var.Make(items.ToArray()).ToTableSelector(opts);

	public static Tag ToTableSelector<T>(this IRoVar<T[]> Δitems, IRwVar<T> Δrx, TableOptions<T> opts) => TableLogic.Make(Δitems, opts, idx => Δrx.V = Δitems.V[idx]);
	public static Tag ToTableSelector<T>(this IEnumerable<T> items, IRwVar<T> Δrx, TableOptions<T> opts) => Var.Make(items.ToArray()).ToTableSelector(Δrx, opts);
}
