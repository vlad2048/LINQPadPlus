namespace LINQPadPlus.Plotly._sys;

static class UpdateCompiler
{
	public static PlotUpdate Compile(Action<IPlotUpdater> fun)
	{
		var updater = new PlotUpdater();
		fun(updater);
		return updater.Update;
	}

	sealed class PlotUpdater : IPlotUpdater
	{
		readonly List<(int, ITrace)> traceUpdates = [];
		Layout? layoutUpdate;

		public PlotUpdate Update => new([..traceUpdates], layoutUpdate);

		public IPlotUpdater Trace(int idx, ITrace trace) => this.With(() => traceUpdates.Add((idx, trace)));
		public IPlotUpdater Layout(Layout layout) => this.With(() => layoutUpdate = layout);
	}
}