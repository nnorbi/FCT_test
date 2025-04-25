using UnityEngine;

[CreateAssetMenu(fileName = "Research Speed", menuName = "Metadata/Research/Speed", order = 4)]
public class MetaResearchSpeed : ScriptableObject
{
	public Sprite Icon;

	public string Title => ("research.speed." + base.name + ".title").tr();
}
