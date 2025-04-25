using Unity.Mathematics;

public class DrawHooks
{
	public delegate void DrawMapDelegate(FrameDrawOptions options, GameMap map);

	public delegate void DrawSuperChunkDelegate(FrameDrawOptions options, MapSuperChunk chunk);

	public delegate void DrawShapeResourceSourceDelegate(FrameDrawOptions options, ShapeResourceSource source);

	public delegate void DrawFluidResourceSourceDelegate(FrameDrawOptions options, FluidResourceSource source);

	public delegate void DrawIslandDelegate(FrameDrawOptions options, Island island);

	public delegate void DrawIslandChunkDelegate(FrameDrawOptions options, IslandChunk chunk);

	public delegate void DrawIslandNotchDelegate(FrameDrawOptions options, IslandChunkNotch notch);

	public delegate void DrawTrainDelegate(FrameDrawOptions options, Train train, float3 pos_W, float angle);

	public delegate void DrawRailDelegate(FrameDrawOptions options, TrainRailNode rail);

	public DrawMapDelegate OnDrawMap = delegate
	{
	};

	public DrawSuperChunkDelegate OnDrawSuperChunk = delegate
	{
	};

	public DrawShapeResourceSourceDelegate OnDrawShapeResourceSource = delegate
	{
	};

	public DrawFluidResourceSourceDelegate OnDrawFluidResourceSource = delegate
	{
	};

	public DrawIslandDelegate OnDrawIslandAlwaysNeedsManualCulling = delegate
	{
	};

	public DrawIslandChunkDelegate OnDrawIslandChunk = delegate
	{
	};

	public DrawIslandNotchDelegate OnDrawIslandNotch = delegate
	{
	};

	public DrawTrainDelegate OnDrawTrain = delegate
	{
	};

	public DrawRailDelegate OnDrawRail = delegate
	{
	};
}
