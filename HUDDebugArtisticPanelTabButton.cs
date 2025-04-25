using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDDebugArtisticPanelTabButton : MonoBehaviour, IDisposable
{
	[SerializeField]
	protected Graphic[] TargetTintGraphics;

	[SerializeField]
	protected Color DefaultColor;

	[SerializeField]
	protected Color AccentColor;

	[SerializeField]
	protected Button Button;

	public UnityEvent OnClicked => Button.onClick;

	public void Dispose()
	{
		OnClicked.RemoveAllListeners();
	}

	public void Show()
	{
		Graphic[] targetTintGraphics = TargetTintGraphics;
		foreach (Graphic graphic in targetTintGraphics)
		{
			graphic.color = AccentColor;
		}
	}

	public void Hide()
	{
		Graphic[] targetTintGraphics = TargetTintGraphics;
		foreach (Graphic graphic in targetTintGraphics)
		{
			graphic.color = DefaultColor;
		}
	}
}
