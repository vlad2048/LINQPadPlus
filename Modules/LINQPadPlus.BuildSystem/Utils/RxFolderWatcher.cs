using System.Reactive.Linq;
using LINQPad;

namespace LINQPadPlus.BuildSystem;

public static class RxFolderWatcher
{
	public static IObservable<string> Watch(string folder, TimeSpan debouncePeriod) =>
		Obs.Using(
			() => new FileSystemWatcher
			{
				Path = folder,
				Filter = "*.*",
				IncludeSubdirectories = true,
				NotifyFilter = NotifyFilters.LastWrite,
				EnableRaisingEvents = true,
			},
			watcher => watcher
				.WhenChanged()
				//.Do(e => $"[{e.ChangeType}]  name:'{e.Name}'  fullpath:'{e.FullPath}'  valid:{IsNameValid(e.Name)}".Dump())
				.Where(e => IsNameValid(e.Name))
				.Select(e => e.FullPath)
				.Throttle(debouncePeriod)
		);

	static bool IsNameValid(this string? f) => f != null && Path.GetFileName(f) != ".git";


	static IObservable<FileSystemEventArgs> WhenChanged(this FileSystemWatcher watcher) =>
		Obs.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(e => watcher.Changed += e, e => watcher.Changed -= e)
			.Select(e => e.EventArgs);


	/*
	/// <summary>
	/// IMPORTANT:
	/// ==========
	/// 
	/// When using it in LINQPad, you need to keep a static reference
	/// to the IDisposable returned when subscribing to the observable.
	/// Otherwise you'll never see any events.
	/// </summary>
	public static IObservable<string> Watch(string folder, TimeSpan debouncePeriod) =>
		Obs.Create<string>(observer =>
		{
			var watcher = new FileSystemWatcher
			{
				Path = folder,
				Filter = "*.*",
				IncludeSubdirectories = true,
				NotifyFilter = NotifyFilters.LastWrite,
				EnableRaisingEvents = true,
			};
			var subscription = Obs.FromEventPattern<FileSystemEventArgs>(watcher, "Changed")
				.Select(e => e.EventArgs)
				.Where(e => e.Name != null)
				.Select(e => e.FullPath)
				.Where(IsRelPathRelevant)
				.Throttle(debouncePeriod)
				.Subscribe(observer).D();
			
			return () =>
			{
				watcher.EnableRaisingEvents = false;
				subscription.Dispose();
			};
		});
	
	

	static bool IsRelPathRelevant(string relPath) =>
		!relPath.Contains(@"\obj") &&
		!relPath.Contains(@"\bin") &&
		!relPath.Contains(@"\.vs");
	*/
}