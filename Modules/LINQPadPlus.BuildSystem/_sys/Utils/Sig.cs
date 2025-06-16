using System.Reactive;
using System.Reactive.Subjects;

namespace LINQPadPlus.BuildSystem._sys.Utils;

sealed class Sig : IObservable<Unit>
{
	readonly Subject<Unit> subj = new();

	public void Trig() => subj.OnNext(Unit.Default);

	public IDisposable Subscribe(IObserver<Unit> obs) => subj.Subscribe(obs);
}