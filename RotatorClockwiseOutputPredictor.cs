using JetBrains.Annotations;

[UsedImplicitly]
public class RotatorClockwiseOutputPredictor : RotatorOutputPredictor
{
	protected override int ClockwiseAmount => 1;
}
