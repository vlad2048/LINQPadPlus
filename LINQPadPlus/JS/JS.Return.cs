using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LINQPadPlus._sys;
using LINQPadPlus._sys.Structs;
using LINQPadPlus._sys.Utils;

namespace LINQPadPlus;

public static partial class JS
{
	public static string Return(
		[LanguageInjection(InjectedLanguage.JAVASCRIPT)] string code,
		Func<string, string>? replFun = null,
		[CallerMemberName] string? srcMember = null,
		[CallerFilePath] string? srcFile = null,
		[CallerLineNumber] int srcLine = 0
	)
	{
		var runId = CtxMap.GetNewRunId();
		code = Fmt(code, replFun);
		// @formatter:off
		var codeFull = $$"""
			try {
				const runId = {{runId}};
			
				{{code.JSIndent(1)}}

			} catch (err) {
				({
					id: '{{JSRunIdentifiers.RuntimeErrorIdentifier}}',
					run_id: runId,
					message: err.message,
					stack: err.stack,
				});
			}
			""";
		// @formatter:on

		var ctx = new CSErrorCtx(true, code, codeFull, new CSharpSourceLocation(srcMember, srcFile, srcLine), DateTime.Now);
		CtxMap.KeepTrackOfCtx(runId, ctx);
		return JSRunLogic.Return(ctx);
	}
}