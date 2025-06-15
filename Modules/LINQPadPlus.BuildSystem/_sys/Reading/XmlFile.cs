using System.Xml.Linq;
using LINQPadPlus.BuildSystem._sys.Reading.Structs;
using LINQPadPlus.BuildSystem._sys.Reading.Xml;
using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys.Reading;


interface IPrjReader
{
	bool GetValueFlag(PrjFlag flag);
	string GetValue(string path);
	T[] GetItems<T>(string path, Func<XElement, T> f);
}

interface IPrjWriter
{
	void SetValueFlag(PrjFlag flag, bool value);
	void SetValue(string path, string value);
}

static class XmlFile
{
	public static T Read<T>(string file, Func<IPrjReader, T> fun)
	{
		var reader = new PrjReader(file);
		return fun(reader);
	}

	public static void Write(string file, Action<IPrjWriter> fun)
	{
		var writer = new PrjWriter(file);
		fun(writer);
		writer.Save();
	}

	sealed class PrjReader(string file) : IPrjReader
	{
		readonly XElement root = XmlQuery.OpenFile(file);

		public bool GetValueFlag(PrjFlag flag) =>
			(
				from elt in root.GetElement(flag.GetPath())
				from val in elt.Value.AsBool()
				select val
			)
			.FailWithDefaultValue(flag);

		public string GetValue(string path) =>
			root.GetElement(path).Ensure($"Failed to find value for '{path}'")
				.Value;

		public T[] GetItems<T>(string path, Func<XElement, T> f) =>
			root.GetElements(path)
				.SelectA(f);
	}

	sealed class PrjWriter(string file) : IPrjWriter
	{
		readonly XElement root = XmlQuery.OpenFile(file);

		public void Save()
		{
			var str = root.Document!.GetSaveString();
			if (str == File.ReadAllText(file)) return;
			File.WriteAllText(file, str);
		}

		public void SetValueFlag(PrjFlag flag, bool value)
		{
			var elt = root.GetElement(flag.GetPath()).Ensure($"Failed to get {flag.GetPath()} in {file}");
			elt.SetValue(value);
		}

		public void SetValue(string path, string value)
		{
			var elt = root.GetElement(path).Ensure($"Failed to find value for '{path}'");
			elt.SetValue(value);
		}
	}
}