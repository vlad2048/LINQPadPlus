namespace LINQPadPlus.Plotly;

public sealed record Legend
{
	public double? X { get; init; }
	public double? Y { get; init; }
	public XAnchor? Xanchor { get; init; }
	public YAnchor? Yanchor { get; init; }
	public AnchorRef? Xref { get; init; }
	public AnchorRef? Yref { get; init; }
	public Orientation? Orientation { get; init; }
}

[EnumStyle(EnumStyle.LowerCase)]
public enum Orientation
{
	V,
	H,
}

[EnumStyle(EnumStyle.LowerCase)]
public enum XAnchor
{
	Auto,
	Left,
	Center,
	Right,
}

[EnumStyle(EnumStyle.LowerCase)]
public enum YAnchor
{
	Auto,
	Top,
	Middle,
	Bottom,
}

[EnumStyle(EnumStyle.LowerCase)]
public enum AnchorRef
{
	Container,
	Paper,
}