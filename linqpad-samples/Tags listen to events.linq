<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <Namespace>LINQPadPlus</Namespace>
</Query>

void OnStart() => LINQPadPlusSetup.Init();	// Always initialize the library in OnStart()

void Main()
{
	// listen to the input event and Dump the text box value
	t.Input.listen("input", "elt => elt.value", e => e.Dump()).Dump();
	
	// there's a variant with no event arguments
	t.Button["Click me!"].listen("click", () => "Thank you".Dump()).Dump();
}

