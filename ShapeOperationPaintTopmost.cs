public class ShapeOperationPaintTopmost : ShapeOperation<ShapeOperationPaintTopmostPayload, string>
{
	protected override string ExecuteInternal(ShapeOperationPaintTopmostPayload input)
	{
		ShapeLogic.UnfoldResult unfolded = ShapeLogic.Unfold(input.Shape.Layers);
		int targetLayer = input.Shape.Layers.Length - 1;
		foreach (ShapePartReference reference in unfolded.References)
		{
			ref ShapePart part = ref reference.Part;
			if (part.Shape.AllowColor && part.Shape.AllowChangingColor && reference.LayerIndex == targetLayer)
			{
				part.Color = input.Color;
			}
		}
		return ShapeLogic.FoldAndHash(unfolded.References, input.Shape.PartCount);
	}
}
