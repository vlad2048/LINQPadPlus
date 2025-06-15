using System.Xml.Linq;
using System.Xml;

namespace LINQPadPlus.BuildSystem._sys.Reading.Xml;

static class XmlSaver
{
	const string IndentStr = "  ";
	static readonly XmlWriterSettings xmlOpt = new()
	{
		OmitXmlDeclaration = true,
		Indent = true,
		IndentChars = IndentStr,
	};

	public static string GetSaveString(this XDocument doc)
	{
		using var sw = new StringWriter();
		using var xw = XmlWriter.Create(sw, xmlOpt);
		doc.Save(xw);
		xw.Flush();
		return sw.ToString().InsertLinesInXml();
	}
	
	

	static string InsertLinesInXml(this string str)
	{
		var lines = str.ToLines();

		var indices = lines
			.Select((line, idx) =>
			{
				var indent = line.GetLeadingIndentCount();
				var isClosingTag = line.IsClosingTag();
				return (ShouldInsertLineBefore(indent, isClosingTag), idx);
			})
			.Where(t => t.Item1)
			.Select(t => t.Item2)
			.Reverse()
			.ToArray();

		var lineList = lines.ToList();
		foreach (var idx in indices)
			lineList.Insert(idx, string.Empty);

		return lineList.FromLines();
	}

	static string[] ToLines(this string str) => str.Split(Environment.NewLine);
	static string FromLines(this IEnumerable<string> strs) => string.Join(Environment.NewLine, strs);
	static bool ShouldInsertLineBefore(int indent, bool isClosingTag) => (indent, isClosingTag) switch
	{
		(1, false) => true,
		(0, true) => true,
		_ => false,
	};
	static bool IsClosingTag(this string s) => s.Trim().StartsWith("</");
	static int GetLeadingIndentCount(this string s)
	{
		var c = 0;
		var i = 0;
		while (i < s.Length - (IndentStr.Length - 1) && s[i..(i + IndentStr.Length)] == IndentStr)
		{
			i += IndentStr.Length;
			c++;
		}
		return c;
	}
}