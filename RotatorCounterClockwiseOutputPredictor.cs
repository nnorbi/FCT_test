using JetBrains.Annotations;

[UsedImplicitly]
public class RotatorCounterClockwiseOutputPredictor : RotatorOutputPredictor
{
	protected override int ClockwiseAmount => -1;
}
