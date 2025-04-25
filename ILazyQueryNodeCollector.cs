public interface ILazyQueryNodeCollector<in T>
{
	void Add(T node, int childrenCount);
}
