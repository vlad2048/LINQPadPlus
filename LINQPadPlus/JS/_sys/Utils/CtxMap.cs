using LINQPadPlus._sys.Structs;

namespace LINQPadPlus._sys.Utils;

static class CtxMap
{
	static readonly TimeSpan maxAge = TimeSpan.FromMinutes(1);
	static readonly Dictionary<int, CSErrorCtx> map = new();
	static int curRunId;

	public static int GetNewRunId() => curRunId++;
	
	public static void KeepTrackOfCtx(int runId, CSErrorCtx ctx)
	{
		TidyupMap();
		map[runId] = ctx;
	}
	public static CSErrorCtx? TryGet(int runId) =>
		map.TryGetValue(runId, out var ctx) switch
		{
			true => ctx,
			false => null,
		};

	static void TidyupMap()
	{
		var now = DateTime.Now;
		var dels = map.Where(kv => now - kv.Value.TimeCreated >= maxAge).SelectA(e => e.Key);
		foreach (var del in dels)
			map.Remove(del);
	}
}