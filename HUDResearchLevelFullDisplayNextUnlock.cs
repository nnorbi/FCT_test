using Core.Dependency;
using UnityEngine;
using UnityEngine.UI;

public class HUDResearchLevelFullDisplayNextUnlock : HUDComponent
{
	[SerializeField]
	protected HUDResearchLevelPreview UILevelPreview;

	[SerializeField]
	protected HUDButton UIUnlockButton;

	[SerializeField]
	protected GameObject UILevelUnlockParent;

	[SerializeField]
	protected Button DebugUnlockButton;

	private int numForceUnlock;

	private float lastForceUnlockTime;

	private ResearchManager ResearchManager;

	private ResearchLevelHandle _level;

	public ResearchLevelHandle Level
	{
		get
		{
			return _level;
		}
		set
		{
			_level = value;
			UILevelPreview.Level = value;
		}
	}

	[Construct]
	private void Construct(ResearchManager researchManager)
	{
		ResearchManager = researchManager;
		AddChildView(UILevelPreview);
		AddChildView(UIUnlockButton);
		UIUnlockButton.Clicked.AddListener(TryUnlock);
		if (DebugUnlockButton != null)
		{
			Object.Destroy(DebugUnlockButton.gameObject);
		}
	}

	protected override void OnDispose()
	{
		UIUnlockButton.Clicked.RemoveListener(TryUnlock);
	}

	protected void TryUnlock()
	{
		ResearchManager.TryUnlock(Level);
	}

	private void ForceUnlock()
	{
		lastForceUnlockTime = Time.time;
		if (++numForceUnlock > 2)
		{
			ResearchManager.TryUnlock(Level, forced: true);
			lastForceUnlockTime = 0f;
		}
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		bool canUnlock = ResearchManager.CanUnlock(Level);
		UILevelUnlockParent.SetActive(canUnlock);
		if (Time.time - lastForceUnlockTime > 0.2f)
		{
			numForceUnlock = 0;
		}
	}
}
