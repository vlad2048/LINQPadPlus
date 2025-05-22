using System.Reactive.Linq;
using LINQPadPlus.Rx._sys;

namespace LINQPadPlus.Rx;

public static class Var
{
	public static IRwVar<T> Make<T>(T value) => new RwVar<T>(value);
	
	public static IRoVar<T> Expr<T>(Func<T> fun)
	{
		var (res, whens) = VarCollector.CallAndCollect(fun);

		var obs = Obs.CombineLatest(whens.Select(e => e.WhenChanged))
			.Select(_ => fun());
		
		var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(whens.SelectA(e => e.CancelToken));

		return new RoVar<T>(obs, cancelSource);
	}


	static U[] SelectA<T, U>(this IEnumerable<T> source, Func<T, U> fun) => source.Select(fun).ToArray();
}