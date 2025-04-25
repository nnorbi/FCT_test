public class ShapeOperationPaint : ShapeOperation<ShapeOperationPaintPayload, string>
{
	protected override string ExecuteInternal(ShapeOperationPaintPayload input)
	{
		ShapeLogic.UnfoldResult unfolded = ShapeLogic.Unfold(input.Shape.Layers);
		foreach (ShapePartReference reference in unfolded.References)
		{
			ref ShapePart part = ref reference.Part;
			if (part.Shape.AllowColor && part.Shape.AllowChangingColor)
			{
				part.Color = input.Color;
			}
		}
		return ShapeLogic.FoldAndHash(unfolded.References, input.Shape.PartCount);
	}
}
