using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "TCMainTutorial", menuName = "Metadata/Tutorial/Tutorial Config")]
public class MetaTutorialConfig : ScriptableObject
{
	[RequiredListLength(1, null)]
	public MetaTutorialGroup[] Groups;

	[RequiredListLength(1, null)]
	public MetaTutorialGroup SkillAcademyGroup;

	public MetaBuildingVariant[] InitialInteractedVariants;
}
