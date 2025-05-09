using LINQPad;
using LINQPadPlus.Rx;

namespace LINQPadPlus;

public static class DumpContainerConverter
{
	public static DumpContainer ToDC<T>(this RwVar<T> rx) => ((RoVar<T>)rx).ToDC();

	public static DumpContainer ToDC<T>(this RoVar<T> rx)
	{
		var dc = new DumpContainer(rx.V);
		rx.Subscribe(e => dc.UpdateContent(e));
		return dc;
	}
}