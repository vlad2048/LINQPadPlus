<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <Namespace>LINQPadPlus</Namespace>
</Query>

void OnStart() => LINQPadPlusSetup.Init();


void Main()
{
	t.Label[
		"Checkbox",
		t.Input.attr("type", "checkbox")
	].Dump();
}

