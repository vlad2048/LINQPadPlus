using LINQPadPlus.BuildSystem._sys.Utils;

namespace LINQPadPlus.BuildSystem._sys.CsProjLogic.Structs;

enum PrjFlag
{
	[PrjFlag("/Project/PropertyGroup/IsPackable", true)]
	IsPackable,
	
	[PrjFlag("/Project/PropertyGroup/GenerateDocumentationFile", false)]
	GenerateDocumentationFile,
}

sealed class PrjFlagAttribute(string path, bool defaultValue) : Attribute
{
	public string Path => path;
	public bool DefaultValue => defaultValue;
}

static class PrjFlagExt
{
	public static string GetPath(this PrjFlag flag) => flag.GetAttributeProp(attr => attr.Path);
	public static bool FailWithDefaultValue(this Maybe<bool> mayVal, PrjFlag flag) => mayVal.FailWith(flag.GetDefaultValue());


	static bool GetDefaultValue(this PrjFlag flag) => flag.GetAttributeProp(attr => attr.DefaultValue);

	static T GetAttributeProp<T>(this PrjFlag flag, Func<PrjFlagAttribute, T> f) => f(flag.GetAttributeOfType<PrjFlagAttribute>());

	static T GetAttributeOfType<T>(this Enum enumVal) where T : Attribute
	{
		var member = enumVal.GetType().GetMember(enumVal.ToString());
		var attrs = member[0].GetCustomAttributes(typeof(T), false) ?? throw new ArgumentException("Failed to get PrjFlagAttribute (null)");
		return attrs.Length switch
		{
			0 => throw new ArgumentException("Failed to get PrjFlagAttribute (zero)"),
			_ => attrs[0] as T ?? throw new ArgumentException("Failed to get PrjFlagAttribute (impossible)"),
		};
	}
}