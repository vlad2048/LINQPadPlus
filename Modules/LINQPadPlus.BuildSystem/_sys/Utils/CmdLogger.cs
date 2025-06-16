using LINQPad;

namespace LINQPadPlus.BuildSystem._sys.Utils;

static class CmdLogger
{
	public static void LogClear(this DumpContainer dc) => dc.ClearContent();
	public static void LogCmd(this DumpContainer dc, string exe, string[] args) => dc.Log($"{exe} {string.Join(' ', args)}");
	public static void LogDone(this DumpContainer dc) => dc.Log("done");
	public static void Log(this DumpContainer dc, string msg) => dc.AppendContent($"[{Timestamp}] {msg}");
	static string Timestamp => $"{DateTime.Now:HH:mm:ss.fff}";
}