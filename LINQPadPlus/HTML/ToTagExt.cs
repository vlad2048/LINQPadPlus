using System.Collections;
using LINQPadPlus._sys.Utils;
using System.Reflection;

namespace LINQPadPlus;

public static class ToTagExt
{
	public static Tag ToTag<T>(this T obj) => obj.Render(true);


	static Tag Render<T>(this T obj, bool isRoot)
	{
		if (obj == null) return RenderNull;

		var type = obj.GetType();
		return (type.IsClass && type != typeof(string)) switch
		{
			false => obj.RenderStruct(),
			true => type.IsArray switch
			{
				false => obj switch
				{
					Tag objTag => objTag,
					_ => new object[] { obj }.ToArr().RenderArray(isRoot, true),
				},
				true => obj.ToArr().RenderArray(isRoot, false),
			},
		};
	}

	static Tag RenderNull => t.Span.cls("null")["null"];

	static Tag RenderStruct<T>(this T obj) => t.Span[$"{obj}"];




	static Tag RenderArray(this object[] arr, bool isRoot, bool isObj)
	{
		if (arr.Length == 0) return RenderEmptyArray;
		var horz = isRoot && isObj;
		var map = BuildMap(arr);
		return horz switch
		{
			false => arr.RenderArrayVert(map),
			true => arr.RenderArrayHorz(map),
		};
	}

	static Tag RenderEmptyArray => t.Table[t.Thead[t.Tr[t.Td.cls("typeheader")["(0 items)"]]]];

	static Tag RenderArrayVert(this object[] arr, ArrPropMap map) =>
		t.Table[[
			t.Thead[[
					t.Tr[[
						map.Columns.SelectA(column =>
							t.Th[column]
						)
					]]
				]],

				t.Tbody[[
					arr.Index().SelectA(row =>
						t.Tr[[
							map.Columns.SelectA(column =>
								map.Props[row.Index].TryGetValue(column, out var prop) switch
								{
									false => t.Td,
									true => t.Td[
										prop.GetValue(row.Item).Render(false)
									],
								}
							),
						]]
					),
				]]
		]];

	static Tag RenderArrayHorz(this object[] arr, ArrPropMap map) =>
		t.Table[[
			t.Tbody[[
				map.Columns.SelectA(column =>
					t.Tr[[
						t.Th[column],
						arr.Index().SelectA(row =>
							map.Props[row.Index].TryGetValue(column, out var prop) switch
							{
								false => t.Td,
								true => t.Td[
									prop.GetValue(row.Item).Render(false)
								],
							}
						)
					]]
				)
			]],
		]];


	static object[] ToArr(this object obj)
	{
		var list = new List<object>();
		var objEnum = (IEnumerable)obj;
		foreach (var item in objEnum)
			list.Add(item);
		return [.. list];
	}





	sealed record ArrPropMap(
		string[] Columns,
		IDictionary<string, PropertyInfo>[] Props
	)
	{
		public object ToDump() => new
		{
			Columns,
			Props = Props.Select(map => map.ToDictionary(
				kv => kv.Key,
				kv => kv.Value.Name
			)),
		};
	}


	static ArrPropMap BuildMap(object[] arr)
	{
		var columns = new List<string>();
		var list = new List<IDictionary<string, PropertyInfo>>();
		foreach (var item in arr)
		{
			var itemProps = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
			list.Add(itemProps.ToDictionary(e => e.Name));
			foreach (var itemProp in itemProps)
				columns.Add(itemProp.Name);
		}
		return new ArrPropMap(
			columns.Distinct().ToArray(),
			[.. list]
		);
	}
}