using Core.Dependency;

public class HUDGeneralSettingsRenderer : HUDAggregatedSettingsRenderer
{
	[Construct]
	private void Construct()
	{
		RenderGroup(Globals.Settings.General, allowReset: false);
		RenderGroup(Globals.Settings.Interface, allowReset: true);
		RenderGroup(Globals.Settings.Camera, allowReset: true);
	}

	public override bool TryLeave()
	{
		return true;
	}
}
