using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreloaderMessageDialog : MonoBehaviour
{
	[SerializeField]
	private TMP_Text UIText;

	[SerializeField]
	private TMP_Text UIVersionText;

	[SerializeField]
	private Button UIQuitButton;

	[SerializeField]
	private Button UIDiscordButton;

	public void Setup(string message)
	{
		Cursor.visible = true;
		UIText.text = message;
		UIVersionText.text = GameEnvironmentManager.DETAILED_VERSION;
		UIQuitButton.onClick.AddListener(Application.Quit);
		UIDiscordButton.onClick.AddListener(delegate
		{
			Application.OpenURL("https://discord.gg/bvq5uGxW8G");
		});
	}
}
