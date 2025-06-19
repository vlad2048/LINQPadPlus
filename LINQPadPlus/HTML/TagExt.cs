namespace LINQPadPlus;

public static class TagExt
{
	public static Tag click(this Tag tag, Action action) => tag.listen("click", action);
}