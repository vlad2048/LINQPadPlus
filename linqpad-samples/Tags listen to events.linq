<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <Namespace>LINQPadPlus</Namespace>
  <Namespace>LINQPadPlus.Rx</Namespace>
</Query>

void OnStart() => LINQPadPlusSetup.Init();	// Always initialize the library in OnStart()

void Main()
{
	// listen to the input event and Dump the text box value
	t.Input.listen("input", "elt => elt.value", e => e.Dump()).Dump();
	
	// there's a variant with no event arguments
	t.Button["Click me!"].listen("click", () => "Thank you".Dump()).Dump();

	// and a more complex example
	var Δmsg = Var.Make("misc").D();
	t.Div[[
		t.Button["A"].click(() => Δmsg.V = "MsgA"),
		t.Button["B"].click(() => Δmsg.V = "MsgB"),
		t.Input
			.on(Δmsg, (t, v) => t.attr("value", v), "(elt, v) => elt.value = v")
			.listen("input", "e => e.value", v => Δmsg.V = v),
		t.Span
			.on(Δmsg, (t, v) => t[v], "(elt, v) => elt.innerText = v")
	]].p();
}

