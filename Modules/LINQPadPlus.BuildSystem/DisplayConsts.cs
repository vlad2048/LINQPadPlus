using LINQPadPlus.BuildSystem._sys.Structs;

namespace LINQPadPlus.BuildSystem;

static class DisplayConsts
{
	public const string TextRed = "#f15858";
	public const string TextGreen = "#4beb52";

	// @formatter:off
	public static readonly Dictionary<PrjStatus, string> StatusText = new()
	{
		{ PrjStatus.NotPackable,    "Not packable" },
		{ PrjStatus.Ready,          "Ready for release" },
		{ PrjStatus.Pending,        "Release pending" },
		{ PrjStatus.UptoDate,       "Upto date" },
		{ PrjStatus.ERROR,          "ERROR" },
	};
	public static readonly Dictionary<PrjStatus, string> StatusColors = new()
	{
		{ PrjStatus.NotPackable,    "#818181" },
		{ PrjStatus.Ready,          "#2bccff" },
		{ PrjStatus.Pending,        "#fd7d2f" },
		{ PrjStatus.UptoDate,       "#36dd68" },
		{ PrjStatus.ERROR,          "#f53e1e" },
	};
	// @formatter:on
}