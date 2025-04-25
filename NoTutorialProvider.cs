using UnityEngine.Events;

public class NoTutorialProvider : ITutorialHighlightProvider
{
	public UnityEvent HighlightChanged { get; } = new UnityEvent();

	public bool IsBuildingVariantHighlighted(MetaBuildingVariant variant)
	{
		return false;
	}

	public bool IsIslandLayoutHighlighted(MetaIslandLayout layout)
	{
		return false;
	}

	public bool IsKeybindingHighlighted(string id)
	{
		return false;
	}
}
