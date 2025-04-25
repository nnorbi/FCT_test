using System;

[Serializable]
public class ResearchUnlockCost
{
	public enum CostType
	{
		Fixed,
		Throughput,
		Free
	}

	public CostType Type = CostType.Free;

	public string DefinitionHash;

	public int AmountFixed;

	public int AmountThroughput;

	public BlueprintCurrency CurrencyValue => Singleton<GameCore>.G.Mode.ResearchConfig.ResearchShapeDefaultValue;

	public bool RequiresShape => Type == CostType.Fixed;

	public bool HasCurrencyValue => Type != CostType.Free && CurrencyValue > BlueprintCurrency.Zero;

	public override string ToString()
	{
		return Type switch
		{
			CostType.Fixed => string.Format("{0}: {1} {2}", "Fixed", AmountFixed, DefinitionHash), 
			CostType.Throughput => string.Format("{0}: {1} {2}", "Throughput", AmountThroughput, DefinitionHash), 
			CostType.Free => "Free", 
			_ => throw new ArgumentOutOfRangeException("Type"), 
		};
	}

	public void Validate()
	{
		if (Type == CostType.Free || !string.IsNullOrEmpty(DefinitionHash))
		{
			return;
		}
		throw new Exception("Invalid goal - Type = " + Type.ToString() + " but no shape set.");
	}
}
