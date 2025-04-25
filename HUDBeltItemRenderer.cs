using System;
using UnityEngine;
using UnityEngine.UI;

public static class HUDBeltItemRenderer
{
	public static void RenderItem(BeltItem item, Transform parent, float size = 100f)
	{
		RenderPreviewTexture(item.RenderToTexture(), parent, size);
	}

	public static void RenderShapeRaw(ShapeDefinition shape, Transform parent, float size = 100f)
	{
		RenderPreviewTexture(shape.RenderToTexture(), parent, size);
	}

	private static void RenderPreviewTexture(Texture preview, Transform parent, float size)
	{
		if (parent == null)
		{
			throw new Exception("Item parent is null, can not render preview texture");
		}
		GameObject obj = new GameObject("CachedTexture")
		{
			layer = LayerMask.NameToLayer("UI")
		};
		obj.transform.parent = parent;
		RawImage img = obj.AddComponent<RawImage>();
		img.texture = preview;
		img.raycastTarget = false;
		img.maskable = false;
		img.material = Globals.Resources.DefaultUISpriteMaterial;
		RectTransform rectT = obj.GetComponent<RectTransform>();
		rectT.sizeDelta = new Vector2(size, size);
		obj.transform.localPosition = new Vector3(0f, 0f, 0f);
		obj.transform.localScale = new Vector3(1f, 1f, 1f);
		obj.transform.localRotation = Quaternion.identity;
	}
}
