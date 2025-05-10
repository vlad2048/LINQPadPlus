using LINQPad;

namespace LINQPadPlus;

public sealed record CSharpSourceLocation(
	string? Member,
	string? File,
	int Line
)
{
	public object ToDump() => Util.Pivot(this);
}