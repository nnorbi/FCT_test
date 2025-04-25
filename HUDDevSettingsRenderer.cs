using Core.Dependency;

public class HUDDevSettingsRenderer : HUDAggregatedSettingsRenderer
{
	public override bool TryLeave()
	{
		return true;
	}

	[Construct]
	private void Construct()
	{
		RenderGroup(Globals.Settings.Dev, allowReset: true);
	}
}
