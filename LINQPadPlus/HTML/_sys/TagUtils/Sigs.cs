using System.Reactive.Subjects;
using System.Reactive;

namespace LINQPadPlus._sys.TagUtils;


interface ISig : IObservable<Unit>
{
	void Trig();
}

sealed class Sig : ISig
{
	readonly Subject<Unit> subj = new();
	bool isTriggered;

	public void Trig()
	{
		if (isTriggered) throw new ArgumentException("SigPreRendered cannot be triggered multiple times");
		isTriggered = true;
		subj.OnNext(Unit.Default);
		subj.OnCompleted();
	}

	public IDisposable Subscribe(IObserver<Unit> obs) => subj.Subscribe(obs);
}

sealed class SigAsync : ISig
{
	readonly AsyncSubject<Unit> subj = new();
	bool isTriggered;

	public void Trig()
	{
		if (isTriggered) throw new ArgumentException("SigPostRendered cannot be triggered multiple times");
		isTriggered = true;
		subj.OnNext(Unit.Default);
		subj.OnCompleted();
	}

	public IDisposable Subscribe(IObserver<Unit> obs) => subj.Subscribe(obs);
}