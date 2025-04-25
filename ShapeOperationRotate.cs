public class ShapeOperationRotate : ShapeOperation<ShapeOperationRotatePayload, string>
{
	protected override string ExecuteInternal(ShapeOperationRotatePayload input)
	{
		int partCount = input.Shape.PartCount;
		ShapeLogic.UnfoldResult unfolded = ShapeLogic.Unfold(input.Shape.Layers);
		foreach (ShapePartReference reference in unfolded.References)
		{
			reference.PartIndex = FastMath.SafeMod(reference.PartIndex + input.AmountClockwise, partCount);
		}
		return ShapeLogic.FoldAndHash(unfolded.References, input.Shape.PartCount);
	}
}
