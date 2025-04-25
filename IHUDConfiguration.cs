using System.Collections.Generic;
using Unity.Core.View;

public interface IHUDConfiguration
{
	IEnumerable<PrefabViewReference<HUDPart>> Parts { get; }
}
