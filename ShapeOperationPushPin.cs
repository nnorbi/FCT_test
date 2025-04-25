using System.Collections.Generic;
using System.Linq;

public class ShapeOperationPushPin : ShapeOperation<ShapeDefinition, PushPinOperationResult>
{
	protected override PushPinOperationResult ExecuteInternal(ShapeDefinition input)
	{
		MetaShapeSubPart pinShape = Singleton<GameCore>.G.Mode.ShapeSubParts.First((MetaShapeSubPart p) => p.Code == 'P');
		int pinPartIndex = 0;
		ShapeLogic.UnfoldResult unfolded = ShapeLogic.Unfold(input.Layers);
		List<ShapePartReference> waste = unfolded.References.Where((ShapePartReference reference) => reference.PartIndex == pinPartIndex).ToList();
		List<ShapePartReference> resultWithPin = unfolded.References.Where((ShapePartReference reference) => reference.PartIndex != pinPartIndex).ToList();
		resultWithPin.Add(new ShapePartReference
		{
			LayerIndex = 0,
			PartIndex = 0,
			Part = new ShapePart
			{
				Color = null,
				Shape = pinShape
			}
		});
		ShapeCollapseResult collapsedResultWithPin = ShapeLogic.Collapse(resultWithPin, input.PartCount, unfolded.FusedReferences);
		List<ShapePartReference> resultWithoutPin = unfolded.References.Where((ShapePartReference reference) => reference.PartIndex != pinPartIndex).ToList();
		ShapeCollapseResult collapsedResultWithoutPin = ShapeLogic.Collapse(resultWithoutPin, input.PartCount, unfolded.FusedReferences);
		return new PushPinOperationResult
		{
			ResultWithPin = collapsedResultWithPin?.ResultDefinition,
			ResultWithoutPin = collapsedResultWithoutPin,
			Waste = ShapeLogic.FoldAndHash(waste, input.PartCount)
		};
	}
}
