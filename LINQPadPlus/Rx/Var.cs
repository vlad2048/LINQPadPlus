using LINQPadPlus.Rx._sys.Expressions;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace LINQPadPlus.Rx;

public static class Var
{
	public static RwVar<T> Make<T>(T init) => new(init, false);

	public static RoVar<T> Expr<T>(Expression<Func<T>> expr) => VarExpr.Expr(expr, D);

	public static RoVar<U> SelectVar<T, U>(this RoVar<T> rx, Func<T, U> fun) => new(rx.Select(fun));
}