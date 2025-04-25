using JetBrains.Annotations;

[UsedImplicitly]
public abstract class RotatorOutputPredictor : ForwarderBuildingPredictor
{
	protected abstract int ClockwiseAmount { get; }

	protected override void Process(in ShapeItem input, out ShapeItem output)
	{
		ShapeDefinition itemDefinition = input.Definition;
		string result = Singleton<GameCore>.G.Shapes.Op_Rotate.Execute(new ShapeOperationRotatePayload
		{
			AmountClockwise = ClockwiseAmount,
			Shape = itemDefinition
		});
		output = Singleton<GameCore>.G.Shapes.GetItemByHash(result);
	}
}
