﻿using LINQPadPlus.Plotly._sys;

namespace LINQPadPlus.Plotly;



public interface IPlotUpdater
{
	IPlotUpdater Trace(int idx, ITrace trace);
	IPlotUpdater Layout(Layout layout);
}



public sealed class Plot : Tag
{
	readonly PlotUpdater keeper;

	public Plot(params ITrace[] traces) : this(traces, null)
	{
	}
	
	public Plot(
		ITrace[] traces,
		Layout? layout = null,
		Config? config = null
	) : base("div")
	{
		keeper = new PlotUpdater(
			this,
			traces,
			layout ?? new Layout(),
			config ?? new Config()
		);
	}
	
	public void Update(Action<IPlotUpdater> fun) => keeper.Update(UpdateCompiler.Compile(fun));

	public string GetDebugCode() => DebugCodeGetter.Get(Id);
}




/*public sealed class PlotOpts
{
	public (FlexValue, FlexValue)? FixXAxis { get; set; }

	PlotOpts()
	{
	}

	internal static PlotOpts Build(Action<PlotOpts>? optFun)
	{
		var opts = new PlotOpts();
		optFun?.Invoke(opts);
		return opts;
	}
}



file static class PlotUtils
{
	public static Layout ApplyFixXAxis(this Layout layout, (FlexValue, FlexValue)? fixXAxis) =>
		fixXAxis switch
		{
			null => layout,
			not null => layout with
			{
				Xaxis = new Axis
				{
					Range = FlexArray.FromValues([fixXAxis.Value.Item1, fixXAxis.Value.Item2]),
				},
			},
		};
}
 
 */









/*
// Inherit from Tag
// ----------------
public sealed class Plot : Tag
{
	//readonly Action<string> Log = Logger.Make(LogCategory.Plot);

	readonly TraceLayoutKeeper traceLayout;


	public IObservable<ClickArgs> WhenClick { get; }

	public static Plot Make(
		ITrace[] traces,
		Layout? layout = null,
		Config? config = null,
		Action<PlotOpts>? optFun = null
	) => new(
		traces,
		layout ?? new Layout(),
		config ?? new Config(),
		PlotOpts.Build(optFun)
	);

	internal Plot(
		ITrace[] traces,
		Layout layout,
		Config config,
		PlotOpts opts
	) : base("div")
	{
		var isRendered = Var.Make(false);
		traceLayout = new TraceLayoutKeeper(
			traces,
			layout,
			isRendered,
			(updateTraces, updateLayout) => _sys.Plotly.Update(
				Id,
				updateTraces,
				updateLayout.ApplyFixXAxis(opts.FixXAxis)
			)
		);

		WhenClick = EventDefs.Click.Make<ClickArgs>(Id, isRendered);



		OnRender = () =>
		{
			//Log("OnRender");

			var (traces_, layout_) = traceLayout.OnDump();
			var js = _sys.Plotly.NewPlot(
				Id,
				traces_,
				layout_.ApplyFixXAxis(opts.FixXAxis),
				config
			);
			JS.Run(js);
			isRendered.V = true;
		};
	}



	public void Update((int, ITrace)[] updatesTrace) => Update(updatesTrace, new Layout());
	public void Update(Layout updateLayout) => Update([], updateLayout);

	public void Update((int, ITrace)[] updatesTrace, Layout updateLayout)
	{
		//Log("UpdateTracesLayout");
		traceLayout.OnUpdate(updatesTrace, updateLayout);
	}

	public void WriteDebugFile() => File.WriteAllText(Consts.PlotlyDebugFile, _sys.Plotly.GetJSCode(Id ?? throw new ArgumentException("Impossible")));
}
*/
