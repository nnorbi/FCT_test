using System.Collections.Generic;
using System.Linq;

public class TutorialConfigHandle
{
	public List<TutorialGroupHandle> Groups { get; }

	public TutorialGroupHandle SkillAcademy { get; }

	public List<MetaBuildingVariant> InitialInteractedVariants { get; }

	public TutorialConfigHandle(MetaTutorialConfig config)
	{
		Groups = config.Groups.Select((MetaTutorialGroup g) => new TutorialGroupHandle(g)).ToList();
		SkillAcademy = new TutorialGroupHandle(config.SkillAcademyGroup);
		InitialInteractedVariants = config.InitialInteractedVariants.ToList();
	}
}
