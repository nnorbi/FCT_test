using System.Collections.Generic;
using System.Linq;

public class ShapeOperationCut : ShapeOperation<ShapeDefinition, ShapeCutResult>
{
	protected override ShapeCutResult ExecuteInternal(ShapeDefinition shape)
	{
		int cutAt = shape.PartCount / 2;
		ShapeLogic.UnfoldResult unfolded = ShapeLogic.Unfold(shape.Layers);
		List<ShapePartReference> leftSide = unfolded.References.Where((ShapePartReference reference) => reference.PartIndex >= cutAt).ToList();
		List<ShapePartReference> rightSide = unfolded.References.Where((ShapePartReference reference) => reference.PartIndex < cutAt).ToList();
		return new ShapeCutResult
		{
			LeftSide = ShapeLogic.Collapse(leftSide, shape.PartCount, unfolded.FusedReferences),
			RightSide = ShapeLogic.Collapse(rightSide, shape.PartCount, unfolded.FusedReferences)
		};
	}
}
