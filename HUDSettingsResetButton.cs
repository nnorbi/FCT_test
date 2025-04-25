using Core.Dependency;
using UnityEngine;
using UnityEngine.Events;

public class HUDSettingsResetButton : HUDComponent
{
	[SerializeField]
	private HUDIconButton UIResetButton;

	public readonly UnityEvent ResetRequested = new UnityEvent();

	public bool Active
	{
		set
		{
			UIResetButton.gameObject.SetActiveSelfExt(value);
		}
	}

	[Construct]
	private void Construct()
	{
		AddChildView(UIResetButton);
		UIResetButton.Clicked.AddListener(ResetRequested.Invoke);
	}

	protected override void OnDispose()
	{
		UIResetButton.Clicked.RemoveListener(ResetRequested.Invoke);
	}
}
