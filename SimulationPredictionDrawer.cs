using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class SimulationPredictionDrawer
{
	private const float TIME_PER_SHAPE = 0.5f;

	private Dictionary<int, int> ShapeInputKeyRemap = new Dictionary<int, int>();

	private readonly int InactiveIndicatorKey = Shader.PropertyToID("output_prediction::InactiveIndicatorKey");

	private readonly int ActiveIndicatorKey = Shader.PropertyToID("output_prediction::ActiveIndicatorKey");

	private readonly int NullIndicatorKey = Shader.PropertyToID("output_prediction::NullIndicatorKey");

	private readonly int LoadingIndicatorKey = Shader.PropertyToID("output_prediction::LoadingIndicatorKey");

	private static float3 CalculateOutputPosition(SimulationPredictionInputLocationKey key)
	{
		float floatingHeightOscillation = HUDTheme.PulseAnimation() * 0.25f;
		float height = floatingHeightOscillation * 0.25f + 0.5f;
		return key.DestinationTile.ToCenter_W() + 0.5f * (WorldDirection)key.Direction + height * WorldDirection.Up;
	}

	private void DrawPlate(FrameDrawOptions drawOptions, float3 position)
	{
		Material material = Singleton<GameCore>.G.Theme.BaseResources.UXShapePredictionMaterial;
		int key = ShapeItem.SUPPORT_MESH.InstancingID;
		ModifyInstancingKey(ref key);
		drawOptions.ShapeInstanceManager.AddInstance(key, ShapeItem.SUPPORT_MESH.Mesh, material, FastMatrix.Translate(in position));
	}

	private void DrawMultiplePredictionsIndicators(FrameDrawOptions drawOptions, int predictionsCount, float3 basePosition, Grid.Direction direction, float shapeRadius, int activeIndex)
	{
		float angularSpacing = math.radians(-15f);
		float directionOffset = math.radians(Grid.DirectionToDegrees(direction));
		float angularOffset = directionOffset + angularSpacing * (float)(predictionsCount - 1) * 0.5f;
		for (int i = 0; i < predictionsCount; i++)
		{
			math.sincos((float)i * angularSpacing - angularOffset, out var sin, out var cos);
			float3 offset = -new float3(cos, 0f, sin) * shapeRadius;
			Matrix4x4 transform = FastMatrix.TranslateScale(basePosition + offset, new float3(0.05f, 0.05f, 0.05f));
			var (key, mat) = SelectKeyMaterial(i, activeIndex);
			drawOptions.Draw3DPlaneWithMaterialInstanced(key, mat, in transform);
		}
	}

	private (int, Material) SelectKeyMaterial(int currentIndex, int activeIndex)
	{
		if (currentIndex == activeIndex)
		{
			return (ActiveIndicatorKey, Singleton<GameCore>.G.Theme.BaseResources.UXShapeOutputPredictionActiveIndicatorMaterial);
		}
		return (InactiveIndicatorKey, Singleton<GameCore>.G.Theme.BaseResources.UXShapeOutputPredictionInactiveIndicatorMaterial);
	}

	public void DrawPredictedOutput(FrameDrawOptions drawOptions, SimulationPredictionInputLocationKey locationKey, SimulationPredictionInputPredictionRange predictionRange, bool drawPlateForEmpty = false)
	{
		float3 outputPosition = CalculateOutputPosition(locationKey);
		Material material = Singleton<GameCore>.G.Theme.BaseResources.UXShapePredictionMaterial;
		if (!predictionRange.HasInput())
		{
			if (drawPlateForEmpty)
			{
				DrawPlate(drawOptions, outputPosition);
			}
			return;
		}
		if (predictionRange.IsDegenerated())
		{
			DrawNullPrediction(drawOptions, outputPosition);
			return;
		}
		IReadOnlyCollection<ShapeItem> predictions = predictionRange.GetShapes();
		float progress = Time.time / 0.5f;
		int currentIndex = (int)math.fmod(progress, predictions.Count);
		ShapeItem currentItem = predictions.Skip(currentIndex).First();
		int instancingKey = currentItem.GetDefaultInstancingKey();
		ModifyInstancingKey(ref instancingKey);
		drawOptions.ShapeInstanceManager.AddInstance(instancingKey, currentItem.GetMesh(), material, FastMatrix.Translate(in outputPosition));
		if (predictions.Count > 1)
		{
			DrawMultiplePredictionsIndicators(drawOptions, predictions.Count, outputPosition, locationKey.Direction, 0.27f, currentIndex);
		}
	}

	private void DrawNullPrediction(FrameDrawOptions drawOptions, float3 outputPosition)
	{
		Material nullMaterial = Singleton<GameCore>.G.Theme.BaseResources.UXShapePredictionNullMaterial;
		DrawPlate(drawOptions, outputPosition);
		Matrix4x4 transform = FastMatrix.TranslateScale(outputPosition + new float3(0f, 0.03f, 0f), new float3(0.27f, 0.27f, 0.27f));
		drawOptions.Draw3DPlaneWithMaterialInstanced(NullIndicatorKey, nullMaterial, in transform);
	}

	private void ModifyInstancingKey(ref int instancingKey)
	{
		if (ShapeInputKeyRemap.TryGetValue(instancingKey, out var overrideKey))
		{
			instancingKey = overrideKey;
			return;
		}
		int modifiedKey = Shader.PropertyToID($"ShapePrediction::{instancingKey}");
		ShapeInputKeyRemap.Add(instancingKey, modifiedKey);
		instancingKey = modifiedKey;
	}

	public void DrawLoadingOutput(FrameDrawOptions draw, SimulationPredictionInputLocationKey output)
	{
		float3 outputPosition = CalculateOutputPosition(output);
		Material loadingMaterial = Singleton<GameCore>.G.Theme.BaseResources.UXShapePredictionLoadingMaterial;
		DrawPlate(draw, outputPosition);
		Matrix4x4 transform = FastMatrix.TranslateScale(outputPosition + new float3(0f, 0.03f, 0f), new float3(0.27f, 0.27f, 0.27f));
		draw.Draw3DPlaneWithMaterialInstanced(LoadingIndicatorKey, loadingMaterial, in transform);
	}
}
