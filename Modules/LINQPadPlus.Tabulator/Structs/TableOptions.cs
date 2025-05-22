using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using LINQPadPlus.Tabulator._sys.Structs;

namespace LINQPadPlus.Tabulator;

public sealed record TableOptions<T>(
	int? Width = null,
	int? Height = 300,
	TableLayout? Layout = null,
	int? PageSize = null
)
{
	readonly List<ColumnOptions<T>> columns = [];
	internal ColumnOptions<T>[]? Columns => columns.Count > 0 ? columns.ToArray() : null;
	public TableOptions<T> Add(Func<T, object> fun, string title, Func<ColumnOptions<T>, ColumnOptions<T>>? build = null) =>
		this.With(() =>
		{
			var opt = new ColumnOptions<T>(fun, title, null);
			build?.Invoke(opt);
			columns.Add(opt);
		});
	public TableOptions<T> Add(Expression<Func<T, object>> expr, Func<ColumnOptions<T>, ColumnOptions<T>>? build = null) =>
		this.With(() =>
		{
			if (!expr.IsSimpleAccessor()) throw new ArgumentException("Expression must be a simple property accessor");
			var opt = new ColumnOptions<T>(expr.Compile(), expr.GetSimpleAccessorName(), expr.GetSimpleAccessorPropertyType());
			build?.Invoke(opt);
			columns.Add(opt);
		});

	internal Tag[] ExtraCtrlsPrepend { get; private set; } = [];
	public TableOptions<T> PrependCtrls(params Tag[] ctrls) => this.With(() => ExtraCtrlsPrepend = ExtraCtrlsPrepend.ConcatA(ctrls));

	internal Tag[] ExtraCtrlsAppend { get; private set; } = [];
	public TableOptions<T> AppendCtrls(params Tag[] ctrls) => this.With(() => ExtraCtrlsAppend = ExtraCtrlsAppend.ConcatA(ctrls));

	readonly List<SearchField<T>> searchFields = [];
	internal SearchField<T>[] SearchFields => [..searchFields];
	public TableOptions<T> Search(Func<T, object> fun, [CallerArgumentExpression(nameof(fun))] string? funName = null) => this.With(() => searchFields.Add(new SearchField<T>(fun, (funName ?? throw new ArgumentException("Impossible")).FmtSearchName())));

	internal bool Dbg_ { get; private set; }
	public TableOptions<T> Dbg() => this.With(() => Dbg_ = true);
	
	

	/*internal bool EnableCellCopy_ { get; private set; }
	public TableOptions<T> EnableCellCopy() => this.With(() => EnableCellCopy_ = true);

	internal bool DisplayRowCount_ { get; private set; }
	public TableOptions<T> DisplayRowCount() => this.With(() => DisplayRowCount_ = true);*/
}



file static class TableOptionsUtils
{
	public static string FmtSearchName(this string e) =>
		e
			.AfterArrow()
			.RemoveEDot();

	static string AfterArrow(this string e)
	{
		var xs = e.Split("=>", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		return xs.Length switch
		{
			2 => xs[1],
			_ => e.Trim(),
		};
	}

	static string RemoveEDot(this string e) => e.Replace("e.", "");
}




file static class TableOptionsExprUtils
{
	public static T[] ConcatA<T>(this IEnumerable<T> first, IEnumerable<T> second) => first.Concat(second).ToArray();



	/*

	Expression<Func<T, V>>
	======================
		Type																			NodeType
		-----------------------------------------------------------------------------------------
		MemberExpression (private: PropertyExpression)									MemberAccess
			Expression=ParameterExpression (private: TypedParameterExpression)			Parameter


	Expression<Func<T, object>>
	===========================
		Type																			NodeType
		--------------------------------------------------------------------------------------------
			UnaryExpression																Convert
				Operand=MemberExpression (private: PropertyExpression)					MemberAccess
					Expression=ParameterExpression (private: TypedParameterExpression)	Parameter

	*/
	public static bool IsSimpleAccessor<T, V>(this Expression<Func<T, V>> expr)
	{
		if (
			expr.Body.Is<MemberExpression>(ExpressionType.MemberAccess, out var exprA) &&
			exprA.Expression != null &&
			exprA.Expression.Is<ParameterExpression>(ExpressionType.Parameter, out _)
		)
			return true;

		if (
			expr.Body.Is<UnaryExpression>(ExpressionType.Convert, out var exprB) &&
			exprB.Operand.Is<MemberExpression>(ExpressionType.MemberAccess, out var exprB2) &&
			exprB2.Expression != null &&
			exprB2.Expression.Is<ParameterExpression>(ExpressionType.Parameter, out _)
		)
			return true;

		return false;
	}




	public static string GetSimpleAccessorName<T>(this Expression<Func<T, object>> expr)
	{
		if (!expr.IsSimpleAccessor()) throw new ArgumentException("Expression must be a simple property accessor");

		var lambda = (LambdaExpression)expr;
		var p = lambda.Parameters.Single();

		var visitor = new NameVisitor(p);
		visitor.Visit(expr);

		if (visitor.Name == null) throw new ArgumentException("Expression name not found");

		return visitor.Name;
	}




	sealed class NameVisitor(ParameterExpression p) : ExpressionVisitor
	{
		public string? Name { get; private set; }
		protected override Expression VisitMember(MemberExpression node)
		{
			if (node.Expression == p)
				Name ??= node.Member.Name;
			return base.VisitMember(node);
		}
	}





	public static Type GetSimpleAccessorPropertyType<T>(this Expression<Func<T, object>> expr)
	{
		if (!expr.IsSimpleAccessor()) throw new ArgumentException("Expression must be a simple property accessor");

		if (
			expr.Body.Is<MemberExpression>(ExpressionType.MemberAccess, out var exprA)
		)
			return exprA.Type;

		if (
			expr.Body.Is<UnaryExpression>(ExpressionType.Convert, out var exprB) &&
			exprB.Operand.Is<MemberExpression>(ExpressionType.MemberAccess, out var exprB2)
		)
			return exprB2.Type;

		throw new ArgumentException("Failed to get property type for simple accessor");
	}




	static bool Is<T>(this Expression expr, ExpressionType exprType, [NotNullWhen(true)] out T? typedExpr) where T : Expression
	{
		if (expr is T typedExpr_ && expr.NodeType == exprType)
		{
			typedExpr = typedExpr_;
			return true;
		}
		else
		{
			typedExpr = null;
			return false;
		}
	}

}