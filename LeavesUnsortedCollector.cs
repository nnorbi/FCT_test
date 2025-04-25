using System.Collections.Generic;

public class LeavesUnsortedCollector<T> : ILazyQueryNodeCollector<T>
{
	public HashSet<T> Leaves = new HashSet<T>();

	public void Add(T node, int childrenCount)
	{
		if (childrenCount == 0)
		{
			Leaves.Add(node);
		}
	}
}
