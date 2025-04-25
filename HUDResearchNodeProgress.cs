using Core.Dependency;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class HUDResearchNodeProgress : HUDComponent
{
	protected enum State
	{
		NotInitialized,
		Waiting,
		Progressing,
		Unlockable,
		Completed
	}

	[SerializeField]
	protected HUDProgressBar UIProgressBar;

	[SerializeField]
	protected TMP_Text UIProgressText;

	[SerializeField]
	protected TMP_Text UIWaitingText;

	[SerializeField]
	protected GameObject UIStateProgressingParent;

	[SerializeField]
	protected GameObject UIStateCompleteParent;

	[SerializeField]
	protected GameObject UIStateWaitingParent;

	protected State CurrentState = State.NotInitialized;

	private ResearchManager ResearchManager;

	private IResearchableHandle _research;

	public IResearchableHandle Research
	{
		get
		{
			return _research;
		}
		set
		{
			CurrentState = State.NotInitialized;
			_research = value;
		}
	}

	public bool AlwaysShowProgressBar { get; set; }

	[Construct]
	private void Construct(ResearchManager researchManager)
	{
		ResearchManager = researchManager;
	}

	protected override void OnDispose()
	{
		CurrentState = State.NotInitialized;
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (Research != null)
		{
			ResearchUnlockCost cost = Research.Cost;
			int required = cost.AmountFixed;
			int stored = math.min(required, ResearchManager.ShapeStorage.GetAmount(cost.DefinitionHash));
			State state = State.Waiting;
			if (ResearchManager.Progress.IsUnlocked(Research))
			{
				state = State.Completed;
			}
			else if (ResearchManager.CanUnlock(Research))
			{
				state = State.Unlockable;
			}
			else if (stored > 0 || AlwaysShowProgressBar)
			{
				state = State.Progressing;
			}
			if (state != CurrentState)
			{
				CurrentState = state;
				UIStateCompleteParent.SetActiveSelfExt(CurrentState == State.Unlockable);
				UIStateWaitingParent.SetActiveSelfExt(CurrentState == State.Waiting);
				UIStateProgressingParent.SetActiveSelfExt(CurrentState == State.Progressing);
			}
			if (CurrentState == State.Progressing)
			{
				float progress = (float)stored / (float)required;
				UIProgressBar.SetProgress(progress);
				UIProgressText.text = StringFormatting.FormatShapeAmountFraction(stored, required);
			}
			else if (CurrentState == State.Waiting)
			{
				UIWaitingText.text = StringFormatting.FormatShapeAmountFraction(stored, required);
			}
		}
	}
}
