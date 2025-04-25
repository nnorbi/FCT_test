using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "RUExample", menuName = "Metadata/Research/Generic Unlock")]
public class MetaGenericResearchUnlock : ScriptableObject, IResearchUnlock
{
	[SerializeField]
	[FormerlySerializedAs("Icon")]
	private Sprite _Icon;

	public Sprite Icon => _Icon;

	public string Title => ("research." + base.name + ".title").tr();
}
