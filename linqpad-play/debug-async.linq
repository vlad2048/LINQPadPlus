<Query Kind="Program">
  <Namespace>LINQPadPlus</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Newtonsoft.Json.Serialization</Namespace>
</Query>

#load "lib"

static readonly JsonSerializerSettings jsonOpt = new()
{
	ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
	NullValueHandling = NullValueHandling.Ignore,
	ContractResolver = new DefaultContractResolver
	{
		NamingStrategy = new SnakeCaseNamingStrategy(),
	},
};

sealed record JSRuntimeError(string Id, string RunId, string Message, string Stack);

void Main()
{
	JS.Run("""
	(async function() {
		try {
			external.log('abtc');
		
		} catch (err) {
			external.log(`runId=${runId}`);
			dispatchError(err, runId);
		}
	})()
	""");
}




