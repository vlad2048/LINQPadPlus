using System.Reactive.Linq;
using System.Reactive;

namespace LINQPadPlus.Rx;

public static class RxExt
{
	public static IObservable<Unit> ToUnit<T>(this IObservable<T> obs) => obs.Select(_ => Unit.Default);
}