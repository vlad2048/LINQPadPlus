using System.Reflection;

namespace LINQPadPlus;

public static class ResourceLoader
{
	public static string Load(Assembly ass, string resourceName)
	{
		using var stream = ass.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Cannot find resource: '{resourceName}'");
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}
}