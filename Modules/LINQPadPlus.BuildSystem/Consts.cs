﻿namespace LINQPadPlus.BuildSystem;

static class Consts
{
	//public const string NugetUrl = "https://api.nuget.org/v3/index.json";
	//public const string NugetTestUrl = "https://apiint.nugettest.org/v3/index.json";
	//public const string NugetUrl = "https://apiint.nugettest.org/v3/index.json";

	public static readonly TimeSpan FileDebouncePeriod = TimeSpan.FromMilliseconds(100);
	public static readonly TimeSpan NugetDebouncePeriod = TimeSpan.FromMinutes(1);
}