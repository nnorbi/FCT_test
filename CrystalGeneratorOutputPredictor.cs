using JetBrains.Annotations;

[UsedImplicitly]
public class CrystalGeneratorOutputPredictor : ForwarderBuildingPredictor
{
	protected override void Process(in ShapeItem input, out ShapeItem output)
	{
		output = input;
	}
}
