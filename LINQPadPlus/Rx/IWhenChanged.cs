using System.Reactive;

namespace LINQPadPlus.Rx;

public interface IWhenChanged : IDisposable
{
	CancellationTokenSource CancelSource { get; }
	CancellationToken CancelToken { get; }
	IObservable<Unit> WhenChanged { get; }
}