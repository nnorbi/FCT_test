using Core.Dependency;
using UnityEngine;

public class HUDGameLogo : HUDComponent
{
	[SerializeField]
	private GameObject UIDemoParent;

	[Construct]
	private void Construct()
	{
		UIDemoParent.SetActiveSelfExt(GameEnvironmentManager.IS_DEMO);
	}

	protected override void OnDispose()
	{
	}
}
