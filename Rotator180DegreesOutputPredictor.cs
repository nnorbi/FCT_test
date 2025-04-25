using JetBrains.Annotations;

[UsedImplicitly]
public class Rotator180DegreesOutputPredictor : RotatorOutputPredictor
{
	protected override int ClockwiseAmount => 2;
}
