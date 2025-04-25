using System.Linq;

public class ShapeOperationGenerateCrystal : ShapeOperation<ShapeOperationGenerateCrystalPayload, string>
{
	protected override string ExecuteInternal(ShapeOperationGenerateCrystalPayload input)
	{
		MetaShapeSubPart crystalShape = Singleton<GameCore>.G.Mode.ShapeSubParts.First((MetaShapeSubPart p) => p.Code == 'c');
		ShapeDefinition copied = input.Shape.CloneUncached();
		ShapeLayer[] layers = copied.Layers;
		for (int layerIndex = 0; layerIndex < copied.Layers.Length; layerIndex++)
		{
			ShapeLayer layer = layers[layerIndex];
			for (int quadIndex = 0; quadIndex < copied.PartCount; quadIndex++)
			{
				ref ShapePart part = ref layer.Parts[quadIndex];
				if (part.IsEmpty || part.Shape.Code == 'P')
				{
					part.Shape = crystalShape;
					part.Color = input.Color;
				}
			}
		}
		return new ShapeDefinition(layers).Hash;
	}
}
