using AngleSharp.Html;
using AngleSharp.Html.Parser;

namespace LINQPadPlus;

public static class HtmlBeautifier
{
	public static string BeautifyHtml(this string s)
	{
		var parser = new HtmlParser();
		var dom = parser.ParseDocument("<html><body></body></html>");
		var document = parser.ParseFragment(s, dom.Body ?? throw new ArgumentException("Error beautifying HTML"));
		using var sw = new StringWriter();
		document.ToHtml(sw, new PrettyMarkupFormatter());
		return sw.ToString().Trim();
	}
}