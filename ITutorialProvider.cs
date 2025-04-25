using UnityEngine.Events;

public interface ITutorialProvider
{
	ITutorialEntry CurrentSkillAcademyTip { get; }

	ITutorialEntry CurrentTutorialGroup { get; }

	ITutorialEntry CurrentTutorialStep { get; }

	UnityEvent CurrentSkillAcademyTipChanged { get; }

	UnityEvent CurrentTutorialChanged { get; }
}
