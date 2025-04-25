using System.Collections.Generic;

public class CrateItemManager
{
	protected Dictionary<string, ShapeCrateItem> ShapeCrateCache = new Dictionary<string, ShapeCrateItem>();

	protected Dictionary<Fluid, FluidCrateItem> FluidCrateCache = new Dictionary<Fluid, FluidCrateItem>();

	public ShapeCrateItem GetShapeCrateByHash(string hash)
	{
		if (string.IsNullOrEmpty(hash))
		{
			return null;
		}
		ShapeCrateCache.TryGetValue(hash, out var cached);
		if (cached != null)
		{
			return cached;
		}
		return ShapeCrateCache[hash] = new ShapeCrateItem(Singleton<GameCore>.G.Shapes.GetDefinitionByHash(hash));
	}

	public FluidCrateItem GetFluidCrate(Fluid fluid)
	{
		if (fluid == null)
		{
			return null;
		}
		FluidCrateCache.TryGetValue(fluid, out var cached);
		if (cached != null)
		{
			return cached;
		}
		return FluidCrateCache[fluid] = new FluidCrateItem(fluid);
	}

	public void OnGameCleanup()
	{
	}
}
