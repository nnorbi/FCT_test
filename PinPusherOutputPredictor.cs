using JetBrains.Annotations;

[UsedImplicitly]
public class PinPusherOutputPredictor : ForwarderBuildingPredictor
{
	protected override void Process(in ShapeItem input, out ShapeItem output)
	{
		PushPinOperationResult result = Singleton<GameCore>.G.Shapes.Op_PushPin.Execute(input.Definition);
		output = Singleton<GameCore>.G.Shapes.GetItemByHash(result.ResultWithPin);
	}
}
