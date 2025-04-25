using System.Collections.Generic;
using System.Linq;

public class ShapeOperationClearAllPinsInternal : ShapeOperation<ShapeDefinition, string>
{
	protected override string ExecuteInternal(ShapeDefinition input)
	{
		ShapeLogic.UnfoldResult unfolded = ShapeLogic.Unfold(input.Layers);
		List<ShapePartReference> resultWithoutPins = unfolded.References.Where((ShapePartReference reference) => reference.Part.Shape.Code != 'P').ToList();
		return ShapeLogic.FoldAndHash(resultWithoutPins, input.PartCount);
	}
}
