<Query Kind="Program">
  <NuGetReference>LINQPadPlus</NuGetReference>
  <Namespace>LINQPadPlus</Namespace>
</Query>

void OnStart() => LINQPadPlusSetup.Init();


void Main()
{
	tag.Label[
		"Checkbox",
		tag.Input.attr("type", "checkbox")
	].Dump();
}

