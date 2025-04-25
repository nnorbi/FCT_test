using System.Collections.Generic;
using Unity.Core.View;
using UnityEngine;

public class HUDConfiguration : ScriptableObject, IHUDConfiguration
{
	[SerializeField]
	private List<PrefabViewReference<HUDPart>> PartReferences = new List<PrefabViewReference<HUDPart>>();

	public IEnumerable<PrefabViewReference<HUDPart>> Parts => PartReferences;
}
