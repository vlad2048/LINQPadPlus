<Query Kind="Program">
  <Reference>D:\dev\big\LINQPadPlus\LINQPadPlus\bin\Debug\net9.0\LINQPadPlus.dll</Reference>
  <Namespace>LINQPadPlus</Namespace>
</Query>

void OnStart()
{
	LINQPadPlusSetup.Init();
	Util.HtmlHead.AddStyles("body { font-family: Consolas; }");
}