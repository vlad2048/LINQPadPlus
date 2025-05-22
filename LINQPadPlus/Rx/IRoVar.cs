using System.Reactive;
using System.Reactive.Linq;
using LINQPadPlus.Rx._sys;

namespace LINQPadPlus.Rx;

public interface IRoVar<out T> : IObservable<T>, IWhenChanged
{
	T V { get; }
}

sealed class RoVar<T> : IRoVar<T>
{
	readonly IObservable<T> obs;
	
	// IWhenChanged
	// ------------
	public CancellationTokenSource CancelSource { get; }
	public CancellationToken CancelToken => CancelSource.Token;
	public void Dispose() => CancelSource.Cancel();
	public IObservable<Unit> WhenChanged => obs.ToUnit();

	// IObservable<T>
	// --------------
	public IDisposable Subscribe(IObserver<T> observer) => obs.Subscribe(observer);

	// IRoVar<T>
	// ---------
	public T V => Task.Run(async () => await obs.FirstAsync()).Result.Collect(this);

	public RoVar(IObservable<T> obs, CancellationTokenSource cancelSource)
	{
		this.obs = obs.Replay(1).RefCount();
		CancelSource = cancelSource;
	}
}