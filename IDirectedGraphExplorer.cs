public interface IDirectedGraphExplorer<T>
{
	void GetAllIncomingNodes(T current, ManagedFixedBuffer<T> fixedBuffer);

	void GetAllOutgoingNodes(T current, ManagedFixedBuffer<T> fixedBuffer);
}
