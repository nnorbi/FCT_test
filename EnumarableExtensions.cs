using System.Collections.Generic;

public static class EnumarableExtensions
{
	public static IEnumerable<T> AsEnumerable<T>(this T item)
	{
		yield return item;
	}
}
