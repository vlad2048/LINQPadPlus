using System.Reactive.Subjects;
using LINQPadPlus.Plotly._sys.Utils;

namespace LINQPadPlus.Plotly._sys;

sealed record PlotUpdate((int, ITrace)[] Traces, Layout? LayoutUpdate)
{
	public bool IsEmpty => Traces.Length == 0 && LayoutUpdate == null;
}

sealed class PlotUpdater
{
	readonly Subject<PlotUpdate> whenUpdate = new();
	readonly ITrace[] traces;
	Layout layout;
	bool isRendered;

	public PlotUpdater(
		Tag tag,
		ITrace[] traces,
		Layout layout,
		Config config
	)
	{
		(this.traces, this.layout) = (traces, layout);

		tag.WhenPostRender.Subscribe(_ =>
		{
			tag.Run(
				"""
				elt => {
					Plotly.newPlot(
						elt,
						____0____,
						____1____,
						____2____
					);
				}
				""",
				e => e
					.JSRepl_ArrOfObj(0, traces)
					.JSRepl_Obj(1, this.layout.PlotlySer())
					.JSRepl_Obj(2, config.PlotlySer())
			);
			isRendered = true;
		});

		whenUpdate.Subscribe(plotUpdate =>
		{
			tag.Run(
				"""
				elt => {
					Plotly.update(
						elt,
						____0____,
						____1____,
						____2____
					);
				}
				""",
				e => e
					.JSRepl_Obj(0, PlotlyJson.PlotlySerNested(plotUpdate.Traces.SelectA(f => f.Item2)))
					.JSRepl_Obj(1, (plotUpdate.LayoutUpdate ?? this.layout).PlotlySer())
					.JSRepl_Arr(2, plotUpdate.Traces.SelectA(f => f.Item1))
			);
		});
	}

	public void Update(PlotUpdate plotUpdate)
	{
		foreach (var (index, traceUpdate) in plotUpdate.Traces)
			traces[index] = traceUpdate;
		
		if (plotUpdate.LayoutUpdate != null)
			layout = plotUpdate.LayoutUpdate;
		
		if (isRendered && !plotUpdate.IsEmpty)
			whenUpdate.OnNext(plotUpdate);
	}
}



file static class DataKeeperFileUtils
{
	public static string JSRepl_ArrOfObj<T>(this string c, int i, T[] xs) => c.JSRepl_Obj(i, xs.PlotlySer());
}