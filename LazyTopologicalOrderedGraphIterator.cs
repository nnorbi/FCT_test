using System;
using System.Collections.Generic;

public class LazyTopologicalOrderedGraphIterator<T>
{
	private readonly IDirectedGraphExplorer<T> GraphExplorer;

	private readonly ILazyQueryNodeCollector<T> LazyQueryNodeCollector;

	private readonly Queue<T> ExplorationQueue = new Queue<T>();

	private readonly HashSet<T> NodesWithDependenciesCleared = new HashSet<T>();

	private readonly HashSet<T> UnresolvedNodes = new HashSet<T>();

	private readonly Stack<T> UnresolvedNodesOrderedNotRemoved = new Stack<T>();

	private readonly HashSet<T> BlacklistedConnections = new HashSet<T>();

	private readonly ManagedFixedBuffer<T> OutgoingBuffer = new ManagedFixedBuffer<T>(4);

	private readonly ManagedFixedBuffer<T> DependenciesBuffer = new ManagedFixedBuffer<T>(4);

	public LazyTopologicalOrderedGraphIterator(T startNode, IDirectedGraphExplorer<T> graphExplorer, ILazyQueryNodeCollector<T> lazyQueryNodeCollector)
	{
		GraphExplorer = graphExplorer;
		LazyQueryNodeCollector = lazyQueryNodeCollector;
		UnresolvedNodes.Clear();
		ExplorationQueue.Clear();
		OutgoingBuffer.Clear();
		DependenciesBuffer.Clear();
		BlacklistedConnections.Clear();
		NodesWithDependenciesCleared.Add(startNode);
		ExplorationQueue.Enqueue(startNode);
	}

	public LazyTopologicalOrderedGraphIterator(IEnumerable<T> startNodes, IDirectedGraphExplorer<T> graphExplorer, ILazyQueryNodeCollector<T> lazyQueryNodeCollector)
	{
		GraphExplorer = graphExplorer;
		LazyQueryNodeCollector = lazyQueryNodeCollector;
		UnresolvedNodes.Clear();
		ExplorationQueue.Clear();
		OutgoingBuffer.Clear();
		DependenciesBuffer.Clear();
		BlacklistedConnections.Clear();
		UnresolvedNodesOrderedNotRemoved.Clear();
		foreach (T startNode in startNodes)
		{
			NodesWithDependenciesCleared.Add(startNode);
			ExplorationQueue.Enqueue(startNode);
		}
	}

	public bool MoveForward(ISimulationPredictionLazyBudget budget)
	{
		while (true)
		{
			if (ExplorationQueue.Count > 0)
			{
				if (budget.BudgetExceeded())
				{
					return false;
				}
				budget.ConsumeOperation();
				T current = ExplorationQueue.Dequeue();
				OutgoingBuffer.Clear();
				GraphExplorer.GetAllOutgoingNodes(current, OutgoingBuffer);
				LazyQueryNodeCollector.Add(current, OutgoingBuffer.Count);
				for (int i = 0; i < OutgoingBuffer.Count; i++)
				{
					T node = OutgoingBuffer[i];
					if (BlacklistedConnections.Contains(node))
					{
						throw new GraphCycleDetectedException();
					}
					if (!NodesWithDependenciesCleared.Contains(node))
					{
						if (AreDependenciesCleared(node))
						{
							ExplorationQueue.Enqueue(node);
							NodesWithDependenciesCleared.Add(node);
							UnresolvedNodes.Remove(node);
						}
						else
						{
							UnresolvedNodes.Add(node);
							UnresolvedNodesOrderedNotRemoved.Push(node);
						}
					}
				}
			}
			else if (!ForgivingLastUnresolvedNode())
			{
				break;
			}
		}
		return true;
	}

	private bool ForgivingLastUnresolvedNode()
	{
		if (UnresolvedNodes.Count == 0)
		{
			return false;
		}
		while (UnresolvedNodesOrderedNotRemoved.Count > 0)
		{
			T nextUnresolvedNodeToForgive = UnresolvedNodesOrderedNotRemoved.Pop();
			if (!UnresolvedNodes.Contains(nextUnresolvedNodeToForgive))
			{
				continue;
			}
			ExplorationQueue.Enqueue(nextUnresolvedNodeToForgive);
			NodesWithDependenciesCleared.Add(nextUnresolvedNodeToForgive);
			BlacklistedConnections.Add(nextUnresolvedNodeToForgive);
			UnresolvedNodes.Remove(nextUnresolvedNodeToForgive);
			return true;
		}
		throw new Exception("Mismatch between nodes ordering stack and hashset");
	}

	private bool AreDependenciesCleared(T current)
	{
		DependenciesBuffer.Clear();
		GraphExplorer.GetAllIncomingNodes(current, DependenciesBuffer);
		for (int i = 0; i < DependenciesBuffer.Count; i++)
		{
			if (!NodesWithDependenciesCleared.Contains(DependenciesBuffer[i]))
			{
				return false;
			}
		}
		return true;
	}
}
