using LINQPad;
using LINQPadPlus.BuildSystem._sys.Structs;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys.GitLogic;

static class GitOps
{
	public static void PushChanges(string folder, string msg, DumpContainer dc)
	{
		try
		{
			PushChangesInternal(folder, msg, dc);
		}
		catch (Exception ex)
		{
			dc.AppendContent(ex);
			throw;
		}
	}
	
	static void PushChangesInternal(string folder, string msg, DumpContainer dc)
	{
		var status = GetStatus(folder, dc);
		dc.Log($"  -> {status}");
		
		void RunAdd() => Cmd.Run("git", folder, ["add", "*"], dc);
		void RunCommit() => Cmd.Run("git", folder, ["commit", "-m", msg], dc);
		void RunPush() => Cmd.Run("git", folder, ["push"], dc);

		switch (status)
		{
			case GitStatus.UnStaged:
				RunAdd();
				RunCommit();
				RunPush();
				break;

			case GitStatus.UnCommited:
				RunCommit();
				RunPush();
				break;

			case GitStatus.UnPushed:
				RunPush();
				break;
			
			case GitStatus.Clean:
				break;
			
			default:
				throw new ArgumentException($"Invalid status: {status}");
		}

		status = GetStatus(folder, dc);
		dc.Log($"  -> {status}");
		if (status != GitStatus.Clean)
			throw new ArgumentException($"GitStatus is not Clean despite our best efforts: {status}");
		dc.LogDone();
	}

	public static GitStatus GetStatus(string folder, DumpContainer dc) =>
		Cmd.RunAndParse("git", folder, ["status"], ParseStatus, dc);

	public static Version[] TagList(string folder, DumpContainer dc) =>
		Cmd.RunAndParse("git", folder, ["tag"], ParseVersions, dc);

	public static void TagCreate(string folder, Version tag, DumpContainer dc)
	{
		Cmd.Run("git", folder, ["tag", "-a", tag.Fmt(), "-m", tag.Fmt()], dc);
		Cmd.Run("git", folder, ["push", "origin", tag.Fmt()], dc);
		dc.LogDone();
	}

	public static void TagDelete(string folder, Version tag, DumpContainer dc)
	{
		Cmd.Run("git", folder, ["push", "origin", "--delete", tag.Fmt()], dc);
		Cmd.Run("git", folder, ["tag", "-d", tag.Fmt()], dc);
		dc.LogDone();
	}


	static GitStatus ParseStatus(string[] xs)
	{
		if (xs.Any(e => e.Contains("Changes not staged for commit") || e.Contains("Untracked files"))) return GitStatus.UnStaged;
		if (xs.Any(e => e.Contains("Changes to be committed"))) return GitStatus.UnCommited;
		if (xs.Any(e => e.Contains("Your branch is ahead of"))) return GitStatus.UnPushed;
		if (xs.Any(e => e.Contains("nothing to commit, working tree clean"))) return GitStatus.Clean;
		throw new ArgumentException($"Could not parse GitStatus\n\n{string.Join(Environment.NewLine, xs)}");
	}


	static Version[] ParseVersions(string[] xs) => xs.Select(ParseVersion).Where(e => e != null).SelectA(e => e!);
	static Version? ParseVersion(string str) =>
		str.StartsWith('v') switch
		{
			true => Version.TryParse(str[1..], out var v) ? v : null,
			false => null,
		};
	static string Fmt(this Version ver) => $"v{ver}";
}