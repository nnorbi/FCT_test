using System.Collections.Generic;

public class ShapeManager
{
	public static float CACHE_EXPIRE_DURATION_SECONDS = 60f;

	public ShapeOperationCut Op_Cut = new ShapeOperationCut();

	public ShapeOperationRotate Op_Rotate = new ShapeOperationRotate();

	public ShapeOperationSwapHalves Op_SwapHalves = new ShapeOperationSwapHalves();

	public ShapeOperationPaint Op_Paint = new ShapeOperationPaint();

	public ShapeOperationPaintTopmost Op_PaintTopmost = new ShapeOperationPaintTopmost();

	public ShapeOperationStack Op_Stack = new ShapeOperationStack();

	public ShapeOperationPushPin Op_PushPin = new ShapeOperationPushPin();

	public ShapeOperationGenerateCrystal Op_GenerateCrystal = new ShapeOperationGenerateCrystal();

	public ShapeOperationUnstack Op_Unstack = new ShapeOperationUnstack();

	public ShapeOperationClearAllPinsInternal Op_ClearAllPinsInternal = new ShapeOperationClearAllPinsInternal();

	protected Dictionary<string, ShapeDefinition> ShapeDefinitionCache = new Dictionary<string, ShapeDefinition>();

	protected Dictionary<string, ShapeItem> ShapeItemCache = new Dictionary<string, ShapeItem>();

	protected List<IBaseShapeOperation> Operations = new List<IBaseShapeOperation>();

	public ShapeManager()
	{
		Operations.Add(Op_Cut);
		Operations.Add(Op_Rotate);
		Operations.Add(Op_SwapHalves);
		Operations.Add(Op_Paint);
		Operations.Add(Op_PaintTopmost);
		Operations.Add(Op_Stack);
		Operations.Add(Op_PushPin);
		Operations.Add(Op_GenerateCrystal);
		Operations.Add(Op_Unstack);
		Operations.Add(Op_ClearAllPinsInternal);
	}

	public ShapeDefinition GetDefinitionByHash(string hash)
	{
		if (string.IsNullOrEmpty(hash))
		{
			return null;
		}
		if (ShapeDefinitionCache.TryGetValue(hash, out var cached))
		{
			return cached;
		}
		return ShapeDefinitionCache[hash] = new ShapeDefinition(hash);
	}

	public ShapeItem GetItemByHash(string hash)
	{
		if (string.IsNullOrEmpty(hash))
		{
			return null;
		}
		if (ShapeItemCache.TryGetValue(hash, out var cached))
		{
			return cached;
		}
		return ShapeItemCache[hash] = new ShapeItem(GetDefinitionByHash(hash));
	}

	public void ClearMeshCaches()
	{
		foreach (ShapeDefinition shape in ShapeDefinitionCache.Values)
		{
			shape.ClearCachedMesh();
		}
		foreach (ShapeItem item in ShapeItemCache.Values)
		{
			item.ClearMeshCache();
		}
	}

	public void GarbageCollect()
	{
		double now = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		foreach (IBaseShapeOperation operation in Operations)
		{
			operation.GarbageCollect(CACHE_EXPIRE_DURATION_SECONDS, now);
		}
	}

	public void OnGameCleanup()
	{
		ClearMeshCaches();
		ShapeDefinitionCache.Clear();
		ShapeItemCache.Clear();
		foreach (IBaseShapeOperation operation in Operations)
		{
			operation.Clear();
		}
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("shapes.clear-cache", delegate(DebugConsole.CommandContext ctx)
		{
			ClearMeshCaches();
			ctx.Output("Shape cache has been cleared");
		});
		console.Register("shapes.stats", delegate(DebugConsole.CommandContext ctx)
		{
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<string, ShapeDefinition> item in ShapeDefinitionCache)
			{
				if (item.Value.HasCachedMesh)
				{
					num2++;
				}
			}
			foreach (KeyValuePair<string, ShapeItem> item2 in ShapeItemCache)
			{
				if (item2.Value.HasCachedMesh)
				{
					num++;
				}
			}
			ctx.Output("Cached Definitions: " + ShapeDefinitionCache.Count + " (" + num2 + " with mesh)");
			ctx.Output("Cached Items: " + ShapeItemCache.Count + " (" + num + " with mesh)");
			ctx.Output("Operation results: ");
			foreach (IBaseShapeOperation current in Operations)
			{
				ctx.Output(" - " + current.GetType().Name + " : " + current.GetCacheSize());
			}
		});
	}
}
