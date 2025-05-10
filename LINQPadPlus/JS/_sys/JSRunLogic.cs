using JetBrains.Annotations;
using LINQPad;
using LINQPadPlus._sys.Structs;
using LINQPadPlus._sys.Utils;

namespace LINQPadPlus._sys;

static class JSRunLogic
{
	static int curRunId;
	public static int GetNextCurId() => curRunId++;
	
	
	public static void Run(
		[LanguageInjection(InjectedLanguage.JAVASCRIPT)] string code,
		CSharpSourceLocation csharpSourceLocation
	)
	{
		var runId = CtxMap.GetNewRunId();

		//const runId = {{runId}};


		// @formatter:off
		var codeFull = $$"""
			try {
				const runId = {{runId}};
				
				{{code.JSIndent(1)}}

				'{{JSRunIdentifiers.ValidRunReturnValueIdentifier}}';
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
		var ctx = new CSErrorCtx(false, code, codeFull, csharpSourceLocation, DateTime.Now);
		CtxMap.KeepTrackOfCtx(runId, ctx);
		Return(ctx);
	}


	public static string Return(CSErrorCtx ctx)
	{
		var resObj = Util.InvokeScript(true, "eval", ctx.CodeFull);
		if (JSErrorUtils.TryGetReturnString(resObj, ctx, out var resStr))
			return resStr;
		var jsError = JSErrorFinder.Find(ctx, resObj);
		throw new JSRunException(jsError, ctx, resObj);
	}


	public static void ErrorReceived(string str)
	{
		var err = JSErrorFinder.CheckRuntimeError(str);
		if (err == null) throw new ArgumentException($"Received an Error but couldn't deserialized it: {str}");
		if (err is not JSRuntimeError runtimeErr) throw new ArgumentException("Received an Error but it was not a JSRuntimeError.");
		if (!runtimeErr.RunId.HasValue) throw new ArgumentException("Received an JSRuntimeError but it has no RunId.");
		var ctx = CtxMap.TryGet(runtimeErr.RunId.Value);
		if (ctx == null) throw new ArgumentException($"Received an JSRuntimeError but it has no context (ctx) in the map (runId:{runtimeErr.RunId}).");
		throw new JSRunException(runtimeErr, ctx, str);
	}
}