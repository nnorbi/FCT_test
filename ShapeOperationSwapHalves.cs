public class ShapeOperationSwapHalves : ShapeOperation<ShapeOperationSwapHalvesPayload, ShapeHalvesSwapResult>
{
	protected override ShapeHalvesSwapResult ExecuteInternal(ShapeOperationSwapHalvesPayload payload)
	{
		ShapeManager shapes = Singleton<GameCore>.G.Shapes;
		ShapeCutResult lowerCutResult = Singleton<GameCore>.G.Shapes.Op_Cut.Execute(payload.LowerShape);
		ShapeCutResult upperCutResult = Singleton<GameCore>.G.Shapes.Op_Cut.Execute(payload.UpperShape);
		ShapeCollapseResult lowerLeftCollapseResult = lowerCutResult.LeftSide;
		ShapeCollapseResult lowerRightCollapseResult = lowerCutResult.RightSide;
		ShapeCollapseResult upperLeftCollapseResult = upperCutResult.LeftSide;
		ShapeCollapseResult upperRightCollapseResult = upperCutResult.RightSide;
		string lowerFinalResult = MergeHalves(shapes.GetDefinitionByHash(upperLeftCollapseResult?.ResultDefinition), shapes.GetDefinitionByHash(lowerRightCollapseResult?.ResultDefinition));
		string upperFinalResult = MergeHalves(shapes.GetDefinitionByHash(upperRightCollapseResult?.ResultDefinition), shapes.GetDefinitionByHash(lowerLeftCollapseResult?.ResultDefinition));
		return new ShapeHalvesSwapResult
		{
			UpperFinalResult = upperFinalResult,
			UpperLeftCollapseResult = upperLeftCollapseResult,
			UpperRightCollapseResult = upperRightCollapseResult,
			LowerFinalResult = lowerFinalResult,
			LowerLeftCollapseResult = lowerLeftCollapseResult,
			LowerRightCollapseResult = lowerRightCollapseResult
		};
	}

	protected string MergeHalves(ShapeDefinition a, ShapeDefinition b)
	{
		if (a == null && b == null)
		{
			return null;
		}
		if (a == null)
		{
			return b.Hash;
		}
		if (b == null)
		{
			return a.Hash;
		}
		ShapeLogic.UnfoldResult partsA = ShapeLogic.Unfold(a.Layers);
		ShapeLogic.UnfoldResult partsB = ShapeLogic.Unfold(b.Layers);
		partsA.References.AddRange(partsB.References);
		return ShapeLogic.FoldAndHash(partsA.References, a.PartCount);
	}
}
