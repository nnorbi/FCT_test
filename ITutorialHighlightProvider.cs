using UnityEngine.Events;

public interface ITutorialHighlightProvider
{
	UnityEvent HighlightChanged { get; }

	bool IsBuildingVariantHighlighted(MetaBuildingVariant variant);

	bool IsIslandLayoutHighlighted(MetaIslandLayout layout);

	bool IsKeybindingHighlighted(string id);
}
