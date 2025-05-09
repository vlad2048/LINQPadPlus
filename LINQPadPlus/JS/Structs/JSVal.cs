using System.Text;
using LINQPadPlus._sys.Utils;

namespace LINQPadPlus;

using I = int;
using D = double;
using B = bool;
using S = string;


public enum JSValType
{
	I,
	D,
	B,
	S,
}

public sealed class JSVal
{
	JSValType Type { get; }
	internal I? I { get; }
	internal D? D { get; }
	internal B? B { get; }
	internal S? S { get; }

	JSVal(I v) => (Type, I) = (JSValType.I, v);
	JSVal(D v) => (Type, D) = (JSValType.D, v);
	JSVal(B v) => (Type, B) = (JSValType.B, v);
	JSVal(S v) => (Type, S) = (JSValType.S, v);

	public object ToDump() => Fmt();
	
	public override string ToString() => Fmt();

	internal S Fmt() =>
		Type switch
		{
			JSValType.I => $"{I}",
			JSValType.D => $"{D}",
			JSValType.B => $"{B}".ToLowerInvariant(),
			JSValType.S => S != null ? S.Quote() : "null",
			_ => throw new ArgumentException("Impossible"),
		};

	internal void WriteAttribute(StringBuilder sb, S key)
	{
		switch (Type)
		{
			case JSValType.B when B == true:
				sb.Append($" {key}");
				break;
			
			case JSValType.B when B == false:
				break;
			
			default:
				sb.Append($" {key}={Fmt()}");
				break;
		}
	}


	public static implicit operator JSVal(I v) => new(v);
	public static implicit operator JSVal(D v) => new(v);
	public static implicit operator JSVal(B v) => new(v);
	public static implicit operator JSVal(S v) => new(v);
}