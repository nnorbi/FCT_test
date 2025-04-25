using UnityEngine;

public static class LayoutUtils
{
	public static void AddGridFillerCell(Transform parent)
	{
		GameObject filler = new GameObject("filler-cell", typeof(RectTransform));
		filler.transform.SetParent(parent);
	}
}
