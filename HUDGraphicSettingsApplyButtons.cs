using Core.Dependency;
using UnityEngine;
using UnityEngine.Events;

public class HUDGraphicSettingsApplyButtons : HUDComponent
{
	[SerializeField]
	private HUDIconButton UIRevertButton;

	[SerializeField]
	private HUDButton UIApplyButton;

	public UnityEvent RevertRequested => UIRevertButton.Clicked;

	public UnityEvent ApplyRequested => UIApplyButton.Clicked;

	public bool Active
	{
		set
		{
			UIRevertButton.gameObject.SetActiveSelfExt(value);
			UIApplyButton.gameObject.SetActiveSelfExt(value);
		}
	}

	[Construct]
	private void Construct()
	{
		AddChildView(UIRevertButton);
		AddChildView(UIApplyButton);
	}

	protected override void OnDispose()
	{
	}
}
