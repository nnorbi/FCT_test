using Unity.Mathematics;

public class CheckpointsDrawer<TCoordinate> : IDrawer<Checkpoint<TCoordinate>> where TCoordinate : IConvertibleToGlobal
{
	private readonly float VerticalOffset;

	private readonly float Scale;

	public CheckpointsDrawer(float verticalOffset = 0.45f, float scale = 1f)
	{
		VerticalOffset = verticalOffset;
		Scale = scale;
	}

	public void Draw(FrameDrawOptions draw, in Checkpoint<TCoordinate> data)
	{
		float3 pos_W = data.Position.ToCenter_W() + VerticalOffset * WorldDirection.Up;
		draw.Draw3DPlaneWithMaterial(draw.Theme.BaseResources.UXHyperBeltPathCheckpointMaterial, FastMatrix.TranslateScale(in pos_W, new float3(1f, 1f, 1f) * Scale));
	}

	void IDrawer<Checkpoint<TCoordinate>>.Draw(FrameDrawOptions draw, in Checkpoint<TCoordinate> data)
	{
		Draw(draw, in data);
	}
}
