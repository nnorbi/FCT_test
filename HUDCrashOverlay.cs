using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDCrashOverlay : MonoBehaviour
{
	public GameObject UIErrorParent;

	public TMP_Text UIErrorText;

	public TMP_Text UIVersionInfoText;

	public Button UIBtnCopyToClipboard;

	public Button UIBtnOpenDiscord;

	protected string CurrentError = null;

	public void Setup(string message)
	{
		Debug.Log("Show error: " + message);
		if (CurrentError == null)
		{
			base.gameObject.SetActive(value: true);
			Cursor.visible = true;
			string versionInfo = "VERSION\n" + GameEnvironmentManager.DETAILED_VERSION + "\n\n";
			versionInfo = versionInfo + "ENVIRONMENT\n" + GameEnvironmentManager.ENVIRONMENT.ToString() + "\n\n";
			versionInfo = versionInfo + "STORE\n" + GameEnvironmentManager.STORE.ToString() + "\n\n";
			versionInfo = versionInfo + "SAVEGAME DIRECTORY\n" + GameEnvironmentManager.SAVEGAME_PATH + "\n\n";
			versionInfo = versionInfo + "SUPPORTED VERSIONS\n" + Savegame.LOWEST_SUPPORTED_VERSION + " ... " + Savegame.VERSION + "\n\n";
			if (UIVersionInfoText != null)
			{
				UIVersionInfoText.text = versionInfo;
			}
			CurrentError = message;
			UIErrorParent.gameObject.SetActive(value: true);
			UIErrorText.text = message;
			UIBtnCopyToClipboard?.onClick.AddListener(delegate
			{
				GUIUtility.systemCopyBuffer = versionInfo + "\n" + message;
			});
			UIBtnOpenDiscord?.onClick.AddListener(delegate
			{
				Application.OpenURL("https://discord.gg/bvq5uGxW8G");
			});
		}
	}
}
