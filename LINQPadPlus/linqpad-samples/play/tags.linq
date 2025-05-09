<Query Kind="Program">
  <Namespace>LINQPad.Controls</Namespace>
  <Namespace>LINQPadPlus</Namespace>
  <Namespace>LINQPadPlus.Rx</Namespace>
</Query>

#load "..\lib"


void Main()
{
	var ΔonA = Var.Make(false);
	var ΔonB = Var.Make(false);
	var ΔonC = Var.Expr(() => ΔonA.V || ΔonB.V);

	var btnA = tag.Button["Toggle A"].listen("click", () => ΔonA.V = !ΔonA.V);
	var btnB = tag.Button["Toggle B"].listen("click", () => ΔonB.V = !ΔonB.V);
	tag.Div[[
		tag.Span[btnA, btnB],
		ΔonA.ToDC(),
		ΔonB.ToDC(),
		ΔonC.ToDC(),
	]].p();
	
}

public static

void Main2()
{
	var btn = new Button("Btn");
	var dc = new DumpContainer("ABC");

	tag.Div[[
		tag.Div["kid1"],
		new[] {
			(HtmlNode)tag.Div["kid2"],
			tag.Div["kid3"].js("elt => elt.style.backgroundColor = 'red'"),
			btn,
			tag.Input.listen("input", "elt => elt.value", e => e.Dump()),
			"123",
			tag.Button["Click me!"].listen("click", () => dc.UpdateContent("Yes")),
			dc,
			tag.Div["kid4"].onReady(() => "ready!".Dump()),
		}.If(true),
		tag.Div["kid5"],
		"MiddleText",
	]].Dump();
}



/*
public static class ToTagExt
{
	public static Tag ToTag(this Control ctrl) => ToTagInner(ctrl);


	static Tag ToTagInner(Control kid)
	{
		kid.HtmlElement.ID = $"id_{Guid.NewGuid()}".Replace("-", "");
		
		var dad = tag.Div.onReady(() => kid.Dump());

		JS.Run(
			"""

		(async () => {
			const [eltDad, eltKid] = await Promise.all([
				window.waitForElement(____0____),
				window.waitForElement(____1____),
			]);
			eltDad.appendChild(eltKid);
		})();
		""",
			e => e
				.JSRepl_Val(0, dad.Id)
				.JSRepl_Val(1, kid.HtmlElement.ID)
		);


		return dad;
	}
}
*/







