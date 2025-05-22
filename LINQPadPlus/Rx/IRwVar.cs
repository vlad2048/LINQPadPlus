using System.Reactive;
using System.Reactive.Subjects;
using LINQPadPlus.Rx._sys;

namespace LINQPadPlus.Rx;

public interface IRwVar<T> : IRoVar<T>
{
	new T V { get; set; }
}

sealed class RwVar<T> : IRwVar<T>
{
	readonly BehaviorSubject<T> subj;

	// IWhenChanged
	// ------------
	public CancellationTokenSource CancelSource { get; } = new();
	public CancellationToken CancelToken => CancelSource.Token;
	public void Dispose() => CancelSource.Cancel();
	public IObservable<Unit> WhenChanged => subj.ToUnit();

	// IObservable<T>
	// --------------
	public IDisposable Subscribe(IObserver<T> observer) => subj.Subscribe(observer);

	// IRwVar<T>
	// ---------
	public T V
	{
		get => subj.Value.Collect(this);
		set => subj.Set(value);
	}

	public RwVar(T value)
	{
		subj = new BehaviorSubject<T>(value);
		subj.OnNext(value);

		CancelToken.Register(() =>
		{
			subj.OnCompleted();
			subj.Dispose();
		});
	}
}


file static class RwVarFileUtils
{
	public static void Set<T>(this BehaviorSubject<T> subj, T value)
	{
		if (Equals(value, subj.Value)) return;
		subj.OnNext(value);
	}
}