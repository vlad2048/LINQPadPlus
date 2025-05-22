﻿using LINQPadPlus.Plotly._sys.JsonConverters;

namespace LINQPadPlus.Plotly;

[EnumStyle(EnumStyle.LowerCase)]
public enum AxisSide
{
	Clockwise,
	Counterclockwise,
	Left,
	Top,
	Bottom,
	Right,
}

[EnumStyle(EnumStyle.LowerCase)]
public enum AxisType
{
	Linear,
	Log,
	Date,
	Category,
	Multicategory,
}

public sealed record Axis
{
	public AxisType? Type { get; init; }
	public bool Autorange { get; init; }
	public bool Fixedrange { get; init; }
	public FlexArray? Range { get; init; }
	public string? Overlaying { get; init; }
	public AxisSide? Side { get; init; }
}