using System.Diagnostics.CodeAnalysis;
// ReSharper disable UnusedMember.Global

namespace LINQPadPlus.BuildSystem._sys;

public abstract class Maybe<T> : IEquatable<Maybe<T>>
{
	public sealed class Some : Maybe<T>
	{
		public T V { get; }
		internal Some(T v)
		{
			V = v;
		}
		public override string ToString() => $"Some<{typeof(T).Name}>({V})";
	}

	public sealed class None : Maybe<T>
	{
		internal None()
		{
		}
		public override string ToString() => $"None<{typeof(T).Name}>()";
	}


	/// <summary>
	/// Allows writing:
	///		from subNode in parent.QueryNode(xPath)
	///		select subNode.InnerText;					// fun = subNode => subNode.InnerText
	///
	/// otherwise you can only 'select subNode', not 'select fun(subNode)'
	///
	/// in FP, this is called map()
	/// </summary>
	public Maybe<V> Select<V>(Func<T, V> fun) =>
		this switch
		{
			Some { V: var val } => May.Some(fun(val)),
			None => May.None<V>(),
			_ => throw new ArgumentException(),
		};

	/// <summary>
	/// Allows writing:
	///		from subNode in parent.QueryNode(xPath)
	///		from attrValue in subNode.GetAttrFromNode(attrName)
	///		select attrValue;
	///
	/// otherwise only a single 'from' statement is allowed
	/// 
	/// in FP, this is called bind()
	/// </summary>
	public Maybe<V> SelectMany<U, V>(Func<T, Maybe<U>> mapper, Func<T, U, V> getResult) =>
		this switch
		{
			Some { V: var val } => mapper(val) switch
			{
				Maybe<U>.Some { V: var valFun } => May.Some(getResult(val, valFun)),
				Maybe<U>.None => May.None<V>(),
				_ => throw new ArgumentException(),
			},
			None => May.None<V>(),
			_ => throw new ArgumentException(),
		};

	public Maybe<T> Where(Func<T, bool> predicate) =>
		this switch
		{
			Some { V: var val } => predicate(val) switch
			{
				true => this,
				false => May.None<T>(),
			},
			None => this,
			_ => throw new ArgumentException(),
		};

	public bool Equals(Maybe<T>? other)
	{
		if (other == null) return false;
		if (other.IsNone() && this.IsNone()) return true;
		if (other.IsSome(out var otherVal) && this.IsSome(out var thisVal))
			return otherVal.Equals(thisVal);
		return false;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((Maybe<T>)obj);
	}

	public override int GetHashCode() => this.IsSome(out var val) switch
	{
		true => val.GetHashCode(),
		false => 0,
	};
	public static bool operator ==(Maybe<T>? left, Maybe<T>? right) => Equals(left, right);
	public static bool operator !=(Maybe<T>? left, Maybe<T>? right) => !Equals(left, right);
}




public static class May
{
	// ************
	// * Creation *
	// ************
	public static Maybe<T> Some<T>(T v) => new Maybe<T>.Some(v);
	public static Maybe<T> None<T>() => new Maybe<T>.None();

	public static Maybe<T> ToMaybe<T>(this T? v) where T : class => v switch
	{
		null => None<T>(),
		not null => Some(v),
	};


	// **************
	// * Unwrapping *
	// **************
	public static bool IsSome<T>(this Maybe<T> may) => may.IsSome(out _);

	public static bool IsSome<T>(this Maybe<T> may, [NotNullWhen(true)] out T? val)
	{
		switch (may)
		{
			case Maybe<T>.Some { V: var valV }:
				val = valV!;
				return true;

			case Maybe<T>.None:
				val = default;
				return false;

			default:
				throw new ArgumentException();
		}
	}

	public static bool IsNone<T>(this Maybe<T> may) => may.IsNone(out _);

	public static bool IsNone<T>(this Maybe<T> may, [NotNullWhen(false)] out T? val)
	{
		switch (may)
		{
			case Maybe<T>.Some { V: var valV }:
				val = valV!;
				return false;

			case Maybe<T>.None:
				val = default;
				return true;

			default:
				throw new ArgumentException();
		}
	}

	public static T Ensure<T>(this Maybe<T> may, string msg) => may.IsSome(out var val) switch
	{
		true => val,
		false => throw new ArgumentException(msg),
	};

	public static T FailWith<T>(this Maybe<T> may, T def) => may.IsSome(out var val) ? val : def;

	public static T[] ToArray<T>(this Maybe<T> may) => may.IsSome(out var val) switch
	{
		true => [val],
		false => [],
	};

	/// <summary>
	/// Converts a Maybe&lt;T&gt; to:
	/// <list type="bullet">
	/// <item><term>A nullable reference</term><description>if T is a reference type</description></item>
	/// <item><term>A nullable value</term><description>if T is a nullable value type</description></item>
	/// <item><term>default(T)</term><description>if T is a non nullable value type</description></item>
	/// </list>
	/// </summary>
	/// <typeparam name="T">Type</typeparam>
	/// <param name="v">Value to convert</param>
	/// <returns>Conversion to nullable</returns>
	public static T? ToNullable<T>(this Maybe<T> v) => v.IsSome(out var val) switch
	{
		true => val,
		false => default,
	};


	// ***********
	// * Testing *
	// ***********
	public static bool IsSomeAndEqualTo<T>(this Maybe<T> may, T elt) => may.IsSome(out var val) switch
	{
		true => val.Equals(elt),
		false => false,
	};
}




