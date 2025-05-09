using System.Linq.Expressions;
using LINQPadPlus.Rx._sys.Expressions.Structs;
using LINQPadPlus.Rx._sys.Expressions.Utils;

namespace LINQPadPlus.Rx._sys.Expressions.Visitors;

sealed class VarPickerExprVisitor : ExpressionVisitor
{
	readonly HashSet<object> addedVars = [];
	readonly List<VarNfo> vars = [];
	public VarNfo[] Vars => vars.ToArray();

	protected override Expression VisitMember(MemberExpression node)
	{
		var valType = Reflex.IsVarVAccessor(node);
		if (valType != null)
		{
			var varNfo = new VarNfo(valType, node);
			var rwVar = varNfo.GetVar();
			if (addedVars.Add(rwVar))
				vars.Add(varNfo);
		}

		return base.VisitMember(node);
	}
}