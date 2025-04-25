using System;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using UnityEngine.Events;

public class TutorialManager : ITutorialProvider, ITutorialHighlightProvider, IDisposable
{
	private Player Player;

	private ResearchManager Research;

	private ITutorialState TutorialState;

	private TutorialEventProcessor EventProcessor;

	private TutorialConditionContext ConditionContext;

	private GameModeHandle GameMode => Singleton<GameCore>.G.Mode;

	private bool IsTutorialEnabled => Globals.Settings.General.Tutorial;

	private IEnumerable<ITutorialEntry> ActiveEntries
	{
		get
		{
			if (CurrentSkillAcademyTip != null)
			{
				yield return CurrentSkillAcademyTip;
			}
			if (CurrentTutorialStep != null)
			{
				yield return CurrentTutorialStep;
			}
		}
	}

	public UnityEvent HighlightChanged { get; } = new UnityEvent();

	public ITutorialEntry CurrentSkillAcademyTip { get; private set; }

	public ITutorialEntry CurrentTutorialGroup { get; private set; }

	public ITutorialEntry CurrentTutorialStep { get; private set; }

	public UnityEvent CurrentSkillAcademyTipChanged { get; } = new UnityEvent();

	public UnityEvent CurrentTutorialChanged { get; } = new UnityEvent();

	public TutorialManager(Player player, ResearchManager research, IEventReceiver passiveEventBus)
	{
		Player = player;
		Research = research;
		TutorialState = Player.TutorialState;
		EventProcessor = new TutorialEventProcessor(player, TutorialState, passiveEventBus);
		ConditionContext = new TutorialConditionContext(Player, Research, TutorialState);
		EventProcessor.Attach();
		CurrentTutorialChanged.AddListener(HighlightChanged.Invoke);
		CurrentSkillAcademyTipChanged.AddListener(HighlightChanged.Invoke);
	}

	public void Dispose()
	{
		EventProcessor.Detach();
	}

	public bool IsBuildingVariantHighlighted(MetaBuildingVariant variant)
	{
		return ActiveEntries.Any((ITutorialEntry entry) => entry.HighlightedBuildingVariants.Contains(variant));
	}

	public bool IsIslandLayoutHighlighted(MetaIslandLayout layout)
	{
		return ActiveEntries.Any((ITutorialEntry entry) => entry.HighlightedIslandLayouts.Contains(layout));
	}

	public bool IsKeybindingHighlighted(string id)
	{
		return ActiveEntries.Any((ITutorialEntry entry) => entry.HighlightedKeybindings.Contains(id));
	}

	public void Init()
	{
		foreach (MetaBuildingVariant entry in GameMode.Tutorial.InitialInteractedVariants)
		{
			TutorialState.TryMarkInteractedWithBuilding(entry);
		}
	}

	private bool AreShowConditionsFullfilled(ITutorialEntry entry)
	{
		if (!IsTutorialEnabled)
		{
			return false;
		}
		if (!entry.DependentResearch.All(Research.Progress.IsUnlocked))
		{
			return false;
		}
		if (entry.ShowConditions.Length != 0)
		{
			ITutorialCondition[] showConditions = entry.ShowConditions;
			foreach (ITutorialCondition condition in showConditions)
			{
				if (!condition.Evaluate(ConditionContext))
				{
					return false;
				}
			}
			return true;
		}
		if (entry.LinkedLevel != null)
		{
			if (!Research.Progress.IsUnlocked(entry.LinkedLevel))
			{
				return false;
			}
			if (!Research.LevelManager.IsLevelRightAfter(entry.LinkedLevel))
			{
				return false;
			}
		}
		return true;
	}

	private bool AreCompleteConditionsFullfilled(ITutorialEntry entry)
	{
		if (entry.CompleteOnResearchComplete != null && Research.Progress.IsUnlocked(entry.CompleteOnResearchComplete))
		{
			return true;
		}
		if (entry.CompleteConditions.Length != 0)
		{
			ITutorialCondition[] completeConditions = entry.CompleteConditions;
			foreach (ITutorialCondition condition in completeConditions)
			{
				if (!condition.Evaluate(ConditionContext))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void Update()
	{
		UpdateSkillAcademy();
		UpdateMainTutorial();
	}

	private void UpdateSkillAcademy()
	{
		UpdateGroup(GameMode.Tutorial.SkillAcademy, out var currentSkillTip);
		if (currentSkillTip != CurrentSkillAcademyTip)
		{
			CurrentSkillAcademyTip = currentSkillTip;
			CurrentSkillAcademyTipChanged.Invoke();
		}
	}

	private void UpdateMainTutorial()
	{
		ITutorialEntry newStep = null;
		ITutorialEntry newGroup = null;
		foreach (TutorialGroupHandle group in GameMode.Tutorial.Groups)
		{
			if (!UpdateGroup(group, out var currentStep))
			{
				continue;
			}
			newGroup = group;
			newStep = currentStep;
			break;
		}
		if (newGroup != CurrentTutorialGroup || newStep != CurrentTutorialStep)
		{
			CurrentTutorialGroup = newGroup;
			CurrentTutorialStep = newStep;
			CurrentTutorialChanged.Invoke();
		}
	}

	private bool UpdateGroup(TutorialGroupHandle group, out TutorialStepHandle currentStep)
	{
		if (!TryUpdateEntry(group))
		{
			currentStep = null;
			return false;
		}
		foreach (TutorialStepHandle entry in group.Steps)
		{
			if (TryUpdateEntry(entry))
			{
				currentStep = entry;
				return true;
			}
		}
		currentStep = null;
		return false;
	}

	private bool TryUpdateEntry(ITutorialEntry entry)
	{
		if (!AreShowConditionsFullfilled(entry))
		{
			return false;
		}
		if (TutorialState.IsEntryCompleted(entry))
		{
			if (!entry.CanInvalidateAfterComplete)
			{
				return false;
			}
			if (AreCompleteConditionsFullfilled(entry))
			{
				return false;
			}
			if (!TutorialState.TryUncompleteEntry(entry))
			{
				throw new Exception("Should not happen.");
			}
		}
		else if (AreCompleteConditionsFullfilled(entry))
		{
			if (!TutorialState.TryCompleteEntry(entry))
			{
				throw new Exception("Should not happen.");
			}
			return false;
		}
		return true;
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("tutorial.stats", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("Current milestone tutorial: " + CurrentTutorialStep?.Id);
			ctx.Output("Current skill academy tutorial: " + CurrentSkillAcademyTip?.Id);
			ctx.Output("Completed steps:");
			TutorialStateSerializedData tutorialStateSerializedData = TutorialState.Serialize();
			string[] completedStepIds = tutorialStateSerializedData.CompletedStepIds;
			foreach (string text in completedStepIds)
			{
				ctx.Output("- " + text);
			}
			ctx.Output("");
			ctx.Output("Completed flags:");
			string[] flags = tutorialStateSerializedData.Flags;
			foreach (string text2 in flags)
			{
				ctx.Output("- " + text2);
			}
			ctx.Output("");
			ctx.Output("Interacted buildings:");
			string[] interactedBuildingsId = tutorialStateSerializedData.InteractedBuildingsId;
			foreach (string text3 in interactedBuildingsId)
			{
				ctx.Output("- " + text3);
			}
		});
		console.Register("tutorial.reset", delegate
		{
			TutorialState.Reset();
		});
	}
}
