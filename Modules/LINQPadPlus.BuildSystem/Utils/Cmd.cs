using CliWrap;
using CliWrap.Buffered;
using LINQPad;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem;

public static class Cmd
{
#if NET9_0_OR_GREATER
	static readonly Lock @lock = new();
#else
	// ReSharper disable once ChangeFieldTypeToSystemThreadingLock
	static readonly object @lock = new();
#endif
	
	public static void Run(string exe, string? folder, string[] args, DumpContainer? dc)
	{
		try
		{
			lock (@lock)
			{
				dc?.LogCmd(exe, args);
				Cli.Wrap(exe)
					.WithWorkingDirectory(folder!)
					.WithArguments(args)
					.ExecuteBufferedAsync()
					.GetAwaiter()
					.GetResult();
			}
		}
		catch (Exception ex)
		{
			dc?.AppendContent(ex);
			throw;
		}
	}

	public static T RunAndParse<T>(string exe, string? folder, string[] args, Func<string[], T> parse, DumpContainer? dc)
	{
		try
		{
			lock (@lock)
			{
				dc?.LogCmd(exe, args);
				var xs = Cli.Wrap(exe)
					.WithWorkingDirectory(folder!)
					.WithArguments(args)
					.ExecuteBufferedAsync()
					.GetAwaiter()
					.GetResult()
					.StandardOutput
					.SplitInLines();
				return parse(xs);
			}
		}
		catch (Exception ex)
		{
			dc?.AppendContent(ex);
			throw;
		}
	}

	static string[] SplitInLines(this string str) => str.Split('\n');
}