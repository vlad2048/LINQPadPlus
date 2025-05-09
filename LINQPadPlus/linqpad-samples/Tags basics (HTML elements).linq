<Query Kind="Program">
  <Namespace>LINQPadPlus</Namespace>
</Query>

#load ".\lib"
//void OnStart() => LINQPadPlusSetup.Init();	// Always initialize the library in OnStart()


void Main()
{
	tag.Label[
		"Checkbox",
		tag.Input.attr("type", "checkbox")
	].Dump();
}

