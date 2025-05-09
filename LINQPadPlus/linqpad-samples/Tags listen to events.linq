<Query Kind="Program">
  <Namespace>LINQPadPlus</Namespace>
</Query>

#load ".\lib"
//void OnStart() => LINQPadPlusSetup.Init();	// Always initialize the library in OnStart()

void Main()
{
	// listen to the input event and Dump the text box value
	tag.Input.listen("input", "elt => elt.value", e => e.Dump()).Dump();
	
	// there's a variant with no event arguments
	tag.Button["Click me!"].listen("click", () => "Thank you".Dump()).Dump();
}

