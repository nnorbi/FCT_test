using System;

[Serializable]
public struct HUDDebugTab
{
	public HUDDebugArtisticPanelTabButton Button;

	public HUDDebugPanelTab Content;

	public void Dispose()
	{
		Button.Dispose();
		if (Content is IDisposable disposableContent)
		{
			disposableContent.Dispose();
		}
	}

	public void Hide()
	{
		Button.Hide();
		Content.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		Button.Show();
		Content.gameObject.SetActive(value: true);
	}
}
