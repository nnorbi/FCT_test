using System.Collections.Generic;
using System.Linq;

public class ShapeOperationUnstack : ShapeOperation<ShapeDefinition, ShapeUnstackResult>
{
	protected override ShapeUnstackResult ExecuteInternal(ShapeDefinition shape)
	{
		ShapeLogic.UnfoldResult unfolded = ShapeLogic.Unfold(shape.Layers);
		int cutOffLayer = shape.Layers.Length - 1;
		List<ShapePartReference> lower = unfolded.References.Where((ShapePartReference reference) => reference.LayerIndex < cutOffLayer).ToList();
		List<ShapePartReference> upper = (from reference in unfolded.References
			where reference.LayerIndex == cutOffLayer
			select new ShapePartReference(reference)
			{
				LayerIndex = 0
			}).ToList();
		return new ShapeUnstackResult
		{
			LowerPart = ShapeLogic.FoldAndHash(lower, shape.PartCount),
			UpperPart = ShapeLogic.FoldAndHash(upper, shape.PartCount)
		};
	}
}
