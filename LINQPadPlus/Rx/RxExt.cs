using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace LINQPadPlus.Rx;

public static class RxExt
{
	public static IObservable<Unit> ToUnit<T>(this IObservable<T> obs) => obs.Select(_ => Unit.Default);

	public static T D<T>(this T obj) where T : IDisposable => obj.D(LINQPadPlusSetup.MainDisp);
	
	public static T D<T>(this T obj, CompositeDisposable disp) where T : IDisposable
	{
		disp.Add(obj);
		return obj;
	}
}