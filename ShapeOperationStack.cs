using System;
using System.Collections.Generic;
using System.Linq;

public class ShapeOperationStack : ShapeOperation<ShapeOperationStackPayload, ShapeStackResult>
{
	protected override ShapeStackResult ExecuteInternal(ShapeOperationStackPayload input)
	{
		ShapeDefinition upperShape = input.UpperShape;
		ShapeDefinition lowerShape = input.LowerShape;
		if (lowerShape.PartCount != upperShape.PartCount)
		{
			throw new Exception("Can not stack shapes with different part count");
		}
		ShapeLogic.UnfoldResult lower = ShapeLogic.Unfold(lowerShape.Layers);
		ShapeLogic.UnfoldResult upper = ShapeLogic.Unfold(upperShape.Layers);
		List<ShapePartReference> combinedReferences = lower.References.ToList();
		combinedReferences.AddRange(upper.References.Select(delegate(ShapePartReference reference)
		{
			ShapePartReference shapePartReference = new ShapePartReference(reference);
			shapePartReference.LayerIndex += Singleton<GameCore>.G.Mode.MaxShapeLayers + 1;
			return shapePartReference;
		}));
		ShapeCollapseResult collapsedShapeResult = ShapeLogic.Collapse(combinedReferences, lowerShape.PartCount);
		return new ShapeStackResult
		{
			Result = collapsedShapeResult
		};
	}
}
