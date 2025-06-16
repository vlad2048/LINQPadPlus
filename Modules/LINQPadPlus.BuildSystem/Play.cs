using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LINQPadPlus.BuildSystem;

public interface IJsoner
{
	static abstract IJsoner I { get; }

	JsonSerializerOptions Opt { get; }
	
	public string Ser<T>(T obj) => JsonSerializer.Serialize(obj, Opt);
	public T Deser<T>(string str) => JsonSerializer.Deserialize<T>(str, Opt) ?? throw new ArgumentException("Deserialization returned null");
	public T Save<T>(T obj, string file) => obj.With(() => File.WriteAllText(file, Ser(obj)));
	public T Load<T>(string file) => Deser<T>(File.ReadAllText(file));
}


public class Jsoner : IJsoner
{
	public static IJsoner I { get; } = new Jsoner();

	public JsonSerializerOptions Opt => new()
	{
		WriteIndented = true,
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		Converters =
		{
			new JsonStringEnumConverter(),
		},
	};

	Jsoner()
	{
	}
}

public class JsonerSpec : IJsoner
{
	public static IJsoner I { get; } = new JsonerSpec();

	public JsonSerializerOptions Opt => new(Jsoner.I.Opt)
	{

	};

	JsonerSpec()
	{
	}
}


static class Prog
{
	public static void Test()
	{
		
	}
}