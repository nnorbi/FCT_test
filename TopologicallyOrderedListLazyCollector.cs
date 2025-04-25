using System.Collections.Generic;

public class TopologicallyOrderedListLazyCollector<T> : ILazyQueryNodeCollector<T>
{
	public List<T> OrderedList = new List<T>();

	public void Add(T node, int childrenCount)
	{
		OrderedList.Add(node);
	}
}
