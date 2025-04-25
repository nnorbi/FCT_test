using System;
using System.Collections.Generic;
using Unity.Mathematics;

public class PathfindingUtils
{
	public static List<int2> LineBresenham(in int2 start, in int2 end, bool axialMovementOnly = false)
	{
		List<int2> result = new List<int2>();
		int2? last = null;
		foreach (int2 point in LineBresenhamInternal(start, end))
		{
			if (last.HasValue && axialMovementOnly)
			{
				int distance = (int)math.distancesq(last.Value, point);
				switch (distance)
				{
				case 2:
					result.Add(new int2(last.Value.x, point.y));
					break;
				default:
				{
					string[] obj = new string[6] { "Pathfinding bresenham bad distance: ", null, null, null, null, null };
					int2? @int = last;
					obj[1] = @int.ToString();
					obj[2] = " -> ";
					int2 int2 = point;
					obj[3] = int2.ToString();
					obj[4] = " -> ";
					obj[5] = distance.ToString();
					throw new Exception(string.Concat(obj));
				}
				case 1:
					break;
				}
			}
			last = point;
			result.Add(point);
		}
		if (!result[0].Equals(start))
		{
			result.Reverse();
		}
		return result;
	}

	private static IEnumerable<int2> LineBresenhamInternal(int2 p0, int2 p1)
	{
		bool steep = math.abs(p1.y - p0.y) > math.abs(p1.x - p0.x);
		if (steep)
		{
			int t = p0.x;
			p0.x = p0.y;
			p0.y = t;
			t = p1.x;
			p1.x = p1.y;
			p1.y = t;
		}
		if (p0.x > p1.x)
		{
			int t2 = p0.x;
			p0.x = p1.x;
			p1.x = t2;
			t2 = p0.y;
			p0.y = p1.y;
			p1.y = t2;
		}
		int dx = p1.x - p0.x;
		int dy = math.abs(p1.y - p0.y);
		int error = dx / 2;
		int ystep = ((p0.y < p1.y) ? 1 : (-1));
		int y = p0.y;
		for (int x = p0.x; x <= p1.x; x++)
		{
			yield return new int2(steep ? y : x, steep ? x : y);
			error -= dy;
			if (error < 0)
			{
				y += ystep;
				error += dx;
			}
		}
	}
}
