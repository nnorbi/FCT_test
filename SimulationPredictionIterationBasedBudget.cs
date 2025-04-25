public class SimulationPredictionIterationBasedBudget : ISimulationPredictionLazyBudget
{
	private readonly int Iterations;

	private int Count;

	public SimulationPredictionIterationBasedBudget(int iterations)
	{
		Iterations = iterations;
	}

	public bool BudgetExceeded()
	{
		return Count > Iterations;
	}

	public void ConsumeOperation()
	{
		Count++;
	}
}
