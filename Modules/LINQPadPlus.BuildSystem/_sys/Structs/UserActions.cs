namespace LINQPadPlus.BuildSystem._sys.Structs;

interface IUsr;
sealed record PushChangesUsr : IUsr;
sealed record BumpVersionUsr : IUsr;
sealed record ReleaseLocallyUsr(string[] PrjFiles) : IUsr;
sealed record ReleaseRemotelyUsr(string[] PrjFiles) : IUsr;


sealed record UserActions(IUsr[] Actions);