using LINQPad;
using LINQPadPlus.Rx;

namespace LINQPadPlus;

public static class DumpContainerConverter
{
	public static DumpContainer ToDC<T>(this IObservable<T> rx)
	{
		var dc = new DumpContainer();
		rx.Subscribe(e => dc.UpdateContent(e)).D();
		return dc;
	}
}