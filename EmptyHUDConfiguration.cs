using System.Collections.Generic;
using Unity.Core.View;

public class EmptyHUDConfiguration : IHUDConfiguration
{
	public IEnumerable<PrefabViewReference<HUDPart>> Parts { get; } = new PrefabViewReference<HUDPart>[0];
}
