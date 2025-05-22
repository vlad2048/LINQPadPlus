namespace LINQPadPlus.Tabulator._sys.Structs;

sealed record SearchField<T>(
	Func<T, object> Fun,
	string Name
);