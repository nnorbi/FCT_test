using Unity.Core.View;

internal interface IHUDContentGroupProvider
{
	THUDSettingsContentGroup Request<THUDSettingsContentGroup>(PrefabViewReference<THUDSettingsContentGroup> prefab) where THUDSettingsContentGroup : HUDComponent, IHUDSettingsContentGroup;

	void Release<THUDSettingsContentGroup>(THUDSettingsContentGroup instance) where THUDSettingsContentGroup : HUDComponent, IHUDSettingsContentGroup;
}
