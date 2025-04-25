public interface ISimulationPredictionLazyBudget
{
	bool BudgetExceeded();

	void ConsumeOperation();
}
