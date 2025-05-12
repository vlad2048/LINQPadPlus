<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <Namespace>LINQPadPlus</Namespace>
</Query>

void OnStart()
{
	LINQPadPlusSetup.Init();	// Always initialize the library in OnStart()
	
	// Import LeaderLine and apply some CSS
	Util.HtmlHead.AddScriptFromUri("https://cdnjs.cloudflare.com/ajax/libs/leader-line/1.0.3/leader-line.min.js");
	Util.HtmlHead.AddStyles("""
	.main {
		position: relative;
		width: 600px;
		height: 400px;
		background-color: #1c4185;
	}
	.src {
		position: absolute;
		left: 50px;
		top: 70px;
		width: 80px;
		height: 40px;
		background-color: #41c726;
	}
	.dst {
		position: absolute;
		left: 320px;
		top: 220px;
		width: 150px;
		height: 90px;
		background-color: #c72c8e;
	}
	""");
}


void Main()
{
	// use Tag.onReady(Action) to execute some C# when the element appears in the DOM
	t.Span["Span"].onReady(() => "I am ready".Dump()).Dump();

	// use Tag.js(string) to execute some JavaScript when the element appears in the DOM
	// in this case we draw some nice arrows between the 2 boxes
	var main = t.Div.cls("main")[[
		t.Div.cls("src"),
		t.Div.cls("dst"),
	]]
	.js("elt => new LeaderLine(elt.childNodes[0], elt.childNodes[1], { size: 8, dash: {animation: true} } )")
	.Dump();
}














