using Core.Dependency;
using UnityEngine;

public abstract class MainMenuState : HUDComponent
{
	protected IMainMenuStateControl Menu;

	protected IHUDDialogStack DialogStack;

	[Construct]
	private void Construct(IMainMenuStateControl menuController, IHUDDialogStack dialogStack)
	{
		Menu = menuController;
		DialogStack = dialogStack;
	}

	protected abstract override void OnDispose();

	public virtual void OnMenuEnterState(object payload)
	{
	}

	public virtual void OnMenuEnterStateCompleted()
	{
	}

	public virtual void OnMenuLeaveState()
	{
	}

	public Vector3 GetVirtualUIWorldPosition()
	{
		return base.gameObject.transform.localPosition;
	}

	public void SetVisibleAndEnabled(bool active)
	{
		base.gameObject.SetActive(active);
	}

	public virtual void GoBack()
	{
	}
}
