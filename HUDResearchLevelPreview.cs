using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDResearchLevelPreview : HUDComponent
{
	[SerializeField]
	protected TMP_Text UITitleText;

	[SerializeField]
	protected HUDShapeDisplay UIRequirementShape;

	[SerializeField]
	protected HUDResearchNodeProgress UIProgress;

	private ShapeManager ShapeManager;

	private ResearchManager ResearchManager;

	private ResearchLevelHandle _Level;

	public ResearchLevelHandle Level
	{
		get
		{
			return _Level;
		}
		set
		{
			if (value != Level)
			{
				_Level = value;
				UpdateView();
			}
		}
	}

	[Construct]
	private void Construct(ShapeManager shapeManager, ResearchManager researchManager)
	{
		ShapeManager = shapeManager;
		ResearchManager = researchManager;
		AddChildView(UIProgress);
		UIProgress.AlwaysShowProgressBar = true;
		AddChildView(UIRequirementShape);
	}

	protected override void OnDispose()
	{
	}

	private void UpdateView()
	{
		if (Level != null)
		{
			UITitleText.text = Level.Meta.Title;
			if (Level.Cost.RequiresShape)
			{
				UIRequirementShape.Shape = ShapeManager.GetDefinitionByHash(Level.Cost.DefinitionHash);
			}
			UIProgress.Research = Level;
		}
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (Level != null)
		{
			UIRequirementShape.Interactable = !ResearchManager.CanUnlock(Level);
		}
	}
}
