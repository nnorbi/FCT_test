public abstract class HUDSidePanelModuleBaseStat
{
	public abstract string GetIconId();

	public abstract string GetTooltipTitle();

	public virtual string GetTooltipText()
	{
		return null;
	}

	public abstract string GetContent();
}
