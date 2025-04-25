using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class CustomExtensions
{
	public static void SetLayerRecursively(this GameObject obj, int layer)
	{
		obj.layer = layer;
		foreach (Transform child in obj.transform)
		{
			child.gameObject.SetLayerRecursively(layer);
		}
	}

	public static float LerpArray(this float[] arr, float factor)
	{
		float intIndex = (int)factor;
		float a = arr[(int)math.clamp(intIndex, 0f, arr.Length - 1)];
		float b = arr[(int)math.clamp(intIndex + 1f, 0f, arr.Length - 1)];
		float f = math.frac(factor);
		return math.lerp(a, b, f);
	}

	public static Color LerpArray(this Color[] arr, float factor)
	{
		float intIndex = (int)factor;
		Color a = arr[(int)math.clamp(intIndex, 0f, arr.Length - 1)];
		Color b = arr[(int)math.clamp(intIndex + 1f, 0f, arr.Length - 1)];
		float f = math.frac(factor);
		return a * (1f - f) + b * f;
	}

	public static T RandomChoice<T>(this T[] arr)
	{
		if (arr.Length == 0)
		{
			return default(T);
		}
		int index = new System.Random().Next(arr.Length);
		return arr[index];
	}

	public static T RandomChoice<T>(this T[] arr, ref Unity.Mathematics.Random rng)
	{
		if (arr.Length == 0)
		{
			return default(T);
		}
		int index = rng.NextInt(arr.Length);
		return arr[index];
	}

	public static void DeleteBySwappingWithLast_ForwardIteration<T>(this List<T> list, ref int index)
	{
		int count = list.Count;
		if (count == 1 || index == count - 1)
		{
			list.RemoveAt(index);
			return;
		}
		list[index] = list[count - 1];
		list.RemoveAt(count - 1);
		index--;
	}

	public static void DeleteLastElement<T>(this List<T> list)
	{
		list.RemoveAt(list.Count - 1);
	}

	public static void RemoveAllChildren(this Transform t, bool keepTaggedChildren = true)
	{
		foreach (Transform child in t)
		{
			bool taggedToKeep = child.name.StartsWith("_Keep_");
			if (!(taggedToKeep && keepTaggedChildren))
			{
				UnityEngine.Object.Destroy(child.gameObject);
			}
		}
	}

	public static void SetActiveSelfExt(this GameObject obj, bool active)
	{
		if (obj.activeSelf != active)
		{
			obj.SetActive(active);
		}
	}

	public static TMP_Text FindText(this GameObject obj, string path)
	{
		return obj.transform.Find(path)?.GetComponent<TMP_Text>();
	}

	public static Image FindImage(this GameObject obj, string path)
	{
		return obj.transform.Find(path)?.GetComponent<Image>();
	}

	public static Button FindButton(this GameObject obj, string path)
	{
		return obj.transform.Find(path)?.GetComponent<Button>();
	}

	public static void InvokeAndClear(this UnityEvent ev)
	{
		ev.Invoke();
		ev.RemoveAllListeners();
	}

	public static void SetInteractableNoFade(this Button btn, bool interactable)
	{
		ColorBlock oldColors = btn.colors;
		ColorBlock newColors = btn.colors;
		newColors.fadeDuration = 0f;
		btn.colors = newColors;
		btn.interactable = interactable;
		btn.colors = oldColors;
	}

	public static void SetSelectOnRight(this Selectable selectable, Selectable selectOnRight)
	{
		Navigation nav = selectable.navigation;
		nav.selectOnRight = selectOnRight;
		selectable.navigation = nav;
	}

	public static void SetSelectOnLeft(this Selectable selectable, Selectable selectOnLeft)
	{
		Navigation nav = selectable.navigation;
		nav.selectOnLeft = selectOnLeft;
		selectable.navigation = nav;
	}

	public static void SetSelectOnUp(this Selectable selectable, Selectable selectOnUp)
	{
		Navigation nav = selectable.navigation;
		nav.selectOnUp = selectOnUp;
		selectable.navigation = nav;
	}

	public static void SetSelectOnDown(this Selectable selectable, Selectable selectOnDown)
	{
		Navigation nav = selectable.navigation;
		nav.selectOnDown = selectOnDown;
		selectable.navigation = nav;
	}

	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		T component = gameObject.GetComponent<T>();
		return (component == null) ? gameObject.AddComponent<T>() : component;
	}

	public static void SetPositionXOnly(this Transform t, float x)
	{
		Vector3 pos = t.position;
		t.position = new Vector3(x, pos.y, pos.z);
	}

	public static void SetLocalPositionXOnly(this Transform t, float x)
	{
		Vector3 pos = t.localPosition;
		t.localPosition = new Vector3(x, pos.y, pos.z);
	}

	public static void SetLocalPositionYOnly(this Transform t, float y)
	{
		Vector3 pos = t.localPosition;
		t.localPosition = new Vector3(pos.x, y, pos.z);
	}

	public static void SetWidth(this RectTransform t, float sizeX)
	{
		t.sizeDelta = new Vector2(sizeX, t.sizeDelta.y);
	}

	public static void SetHeight(this RectTransform t, float sizeY)
	{
		t.sizeDelta = new Vector2(t.sizeDelta.x, sizeY);
	}
}
