global using Obs = System.Reactive.Linq.Observable;

namespace LINQPadPlus;

public static class LINQPadPlusSetup
{
	public static void Init()
	{
		JS.Init();
		Events.Init();
	}
}