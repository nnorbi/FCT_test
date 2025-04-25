using Core.Dependency;
using UnityEngine;
using UnityEngine.UI;

public class HUDShapeDisplay : HUDComponent
{
	[SerializeField]
	private Button UIShowViewerButton;

	[SerializeField]
	private Image UIMainRaycastImage;

	[SerializeField]
	private RectTransform UIShapeParent;

	private HUDEvents Events;

	private ShapeDefinition _shape;

	public ShapeDefinition Shape
	{
		get
		{
			return _shape;
		}
		set
		{
			if (_shape != value)
			{
				_shape = value;
				UpdateView();
			}
		}
	}

	public bool Interactable
	{
		set
		{
			UIMainRaycastImage.raycastTarget = value;
		}
	}

	[Construct]
	private void Construct(HUDEvents hudEvents)
	{
		Events = hudEvents;
		UIShowViewerButton.onClick.AddListener(ShowShapeViewer);
	}

	protected override void OnDispose()
	{
		UIShowViewerButton.onClick.RemoveListener(ShowShapeViewer);
		UIShapeParent.RemoveAllChildren();
	}

	protected void ShowShapeViewer()
	{
		if (Shape == null)
		{
			Globals.UISounds.PlayError();
			return;
		}
		Globals.UISounds.PlayClick();
		Events.ShowShapeViewer.Invoke(Shape);
	}

	public void UpdateView()
	{
		UIShapeParent.RemoveAllChildren();
		if (Shape == null)
		{
			base.Logger.Warning?.Log("Passed empty shape definition to HUDShapeDisplay");
		}
		else
		{
			HUDBeltItemRenderer.RenderShapeRaw(Shape, UIShapeParent, UIShapeParent.sizeDelta.x * 1.3f);
		}
	}
}
