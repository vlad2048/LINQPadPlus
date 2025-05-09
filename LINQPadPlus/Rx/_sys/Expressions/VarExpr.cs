using System.Linq.Expressions;
using System.Reactive.Linq;
using LINQPadPlus.Rx._sys.Expressions.Utils;

namespace LINQPadPlus.Rx._sys.Expressions;

static class VarExpr
{
	public static RoVar<T> Expr<T>(Expression<Func<T>> expr, Disp d)
	{
		var vars = ExprUtils.PickVars(expr);
		var genLambda = ExprUtils.CreateGenLambda(expr, vars);
		object[] ParamsFun() => vars.Select(e => e.GetVal()).ToArray();

		var roVars = vars.Select(e => e.GetVar()).ToArray();
		var whenChangeArr = roVars.Select(Reflex.GetWhenVarChange).ToArray();
		var whenAnyChange = Obs.Merge(whenChangeArr);

		var varArr = ExprUtils.PrecompileLambda(genLambda);
		T Calc() => ExprUtils.EvalLambda<T>(varArr, ParamsFun);
		
		return new RoVar<T>(whenAnyChange.Select(_ => Calc()));
	}
}