using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem;

/*public enum PrjState
{
	Never,
	Ready,
	Pending,
	UptoDate,
	ERROR,
}

public sealed record PrjStates(
	string[] All,
	string[] IsNotPackable,
	string[] NeverReleased,
	string[] ReadyForRelease,
	string[] Pending,
	string[] UptoDate,
	string[] Error_NugetVersion_HigherThan_SolutionVersion
)
{
	public object ToDump() => All
		.SelectA(e => new
		{
			Name = e,
			State = this.Get(e),
		});
}*/




/*
public sealed record PrjReleaseState(
	string Name,
	PrjReleaseStatus Status,
	Version? VersionReleased,
	Version? VersionPending
)
{
}



public static class PrjStatesUtils
{
	public static PrjState Get(this PrjStates set, string x)
	{
		if (set.NeverReleased.Contains(x)) return PrjState.Never;
		if (set.ReadyForRelease.Contains(x)) return PrjState.Ready;
		if (set.Pending.Contains(x)) return PrjState.Pending;
		if (set.UptoDate.Contains(x)) return PrjState.UptoDate;
		if (set.Error_NugetVersion_HigherThan_SolutionVersion.Contains(x)) return PrjState.ERROR;
		throw new ArgumentException($"Invalid project state for: {x}");
	}

	internal static PrjStates Validate(this PrjStates set)
	{
		var xs = set.All.OrderBy(e => e).ToArray();
		var ys =
			set.IsNotPackable
				.Concat(set.NeverReleased)
				.Concat(set.ReadyForRelease)
				.Concat(set.Pending)
				.Concat(set.UptoDate)
				.Concat(set.Error_NugetVersion_HigherThan_SolutionVersion)
				.OrderBy(e => e).ToArray();

		if (!xs.SequenceEqual(ys))
			throw new ArgumentException($"Error - inconsistent classified projects: [{string.Join(", ", xs)}] vs [{string.Join(", ", ys)}]");

		return set;
	}
}
*/
