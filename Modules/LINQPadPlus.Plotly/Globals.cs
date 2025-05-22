global using Obs = System.Reactive.Linq.Observable;
using LINQPad;

namespace LINQPadPlus.Plotly;

public static class LINQPadPlusPlotlySetup
{
	public static void Init()
	{
		Util.HtmlHead.AddScriptFromUri(Consts.PlotlyUrl);

		JS.Run(
			"""
			Plotly.setPlotConfig({
			    plotlyServerURL: 'https://chart-studio.plotly.com',
			    displaylogo: false,
			    modeBarButtonsToRemove: ['toImage', 'select2d', 'lasso2d', 'zoomIn2d', 'zoomOut2d' ],
			    logging: 2,
			    notifyOnLogging: 2,
			}); 
			"""
		);
	}
}