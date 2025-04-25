using System.Collections.Generic;

public static class DrawExtensions
{
	public static void DrawBatch<T>(this IDrawer<T> drawer, FrameDrawOptions draw, IEnumerable<T> batch)
	{
		foreach (T item in batch)
		{
			drawer.Draw(draw, item);
		}
	}
}
