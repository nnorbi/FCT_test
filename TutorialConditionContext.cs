public struct TutorialConditionContext
{
	public Player Player;

	public ResearchManager Research;

	public ITutorialStateReadAccess TutorialState;

	public TutorialConditionContext(Player player, ResearchManager research, ITutorialStateReadAccess tutorialState)
	{
		Player = player;
		Research = research;
		TutorialState = tutorialState;
	}
}
