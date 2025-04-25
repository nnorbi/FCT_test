using UnityEngine;
using UnityEngine.UI;

public class HUDWikiComponentImage : HUDComponent
{
	[SerializeField]
	private Image UIImage;

	[SerializeField]
	private LayoutElement UILayoutElement;

	public Sprite Sprite
	{
		set
		{
			UIImage.sprite = value;
			UILayoutElement.preferredHeight = 767f / value.bounds.size.x * value.bounds.size.y;
		}
	}

	protected override void OnDispose()
	{
	}
}
