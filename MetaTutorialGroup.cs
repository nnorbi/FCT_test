using UnityEngine;

[CreateAssetMenu(fileName = "TGExampleGroup", menuName = "Metadata/Tutorial/Tutorial Group")]
public class MetaTutorialGroup : MetaTutorialEntry
{
	[Header("Tutorial Contents")]
	[Space(20f)]
	public MetaTutorialStep[] Steps;
}
