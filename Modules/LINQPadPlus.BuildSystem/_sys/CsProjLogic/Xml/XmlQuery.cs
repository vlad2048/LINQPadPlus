using System.Xml.Linq;
using System.Xml.XPath;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys.CsProjLogic.Xml;

static class XmlQuery
{
	public static XElement OpenFile(string file) => XDocument.Load(file).Root ?? throw new ArgumentException($"Failed to open XML file: '{file}'");

	/// <summary>
	/// <para><c>/Project/ItemGroup/PackageReference</c></para>
	/// <para><c>//PackageReference</c></para>
	/// </summary>
	public static XElement[] GetElements(this XElement root, string xpath) => root.XPathSelectElements(xpath).ToArray();


	/// <summary>
	/// <para><c>/Project/PropertyGroup/TargetFrameworks</c></para>
	/// </summary>
	public static Maybe<XElement> GetElement(this XElement root, string xpath) => root.XPathSelectElement(xpath).ToMaybe();

	public static string GetAttr(this XElement node, string name) => (node.Attributes(name).FirstOrDefault() ?? throw new ArgumentException($"Failed to get attribute: {name}")).Value;


	public static Maybe<bool> AsBool(this string val) => Lift<bool>(bool.TryParse)(val);
	public static Maybe<Version> AsVersion(this string val) => Lift<Version>(Version.TryParse)(val);


	delegate bool TryParseFun<T>(string s, out T? v);
	delegate Maybe<T> ParseFun<T>(string s);
	static ParseFun<T> Lift<T>(TryParseFun<T> f) =>
		str => f(str, out var val) switch
		{
			false => May.None<T>(),
			true => May.Some(val ?? throw new ArgumentException("Impossible (?)")),
		};
}