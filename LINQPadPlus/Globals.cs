global using Obs = System.Reactive.Linq.Observable;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LINQPadQuery")]

namespace LINQPadPlus;

public static class LINQPadPlusSetup
{
	static CompositeDisposable? mainDisp;
	internal static CompositeDisposable MainDisp => mainDisp ?? throw new ArgumentException("You need to call LINQPadPlusBuildSystemSetup.Init() in OnStart()");


	public static void Init()
	{
		mainDisp?.Dispose();
		mainDisp = new CompositeDisposable();

		JS.Init();
		Events.Init();
	}
}