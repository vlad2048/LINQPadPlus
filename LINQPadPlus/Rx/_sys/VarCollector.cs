namespace LINQPadPlus.Rx._sys;

static class VarCollector
{
	static HashSet<IWhenChanged>? set;

	public static T Collect<T>(this T value, IWhenChanged whenChanged)
	{
		set?.Add(whenChanged);
		return value;
	}

	public static (T, IWhenChanged[]) CallAndCollect<T>(Func<T> fun)
	{
		Enable();
		var res = fun();
		var arr = Disable();
		return (res, arr);
	}

	static void Enable()
	{
		set = [];
	}

	static IWhenChanged[] Disable()
	{
		if (set == null) throw new ArgumentException("set should not be null here");
		var res = set.ToArray();
		set = null;
		return res;
	}
}