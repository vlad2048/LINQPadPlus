using JetBrains.Annotations;
using LINQPad;

namespace LINQPadPlus._sys.Utils;

static class CssUtils
{
	public static void AddStyles([LanguageInjection(InjectedLanguage.CSS)] string css) => Util.HtmlHead.AddStyles(css);
}