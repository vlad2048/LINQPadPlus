namespace LINQPadPlus.BuildSystem._sys.Structs;

sealed record SlnNugetState(
	IReadOnlyDictionary<string, Version> VersionsReleased,
	IReadOnlyDictionary<string, Version> VersionsPending
);