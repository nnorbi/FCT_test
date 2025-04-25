using TMPro;
using UnityEngine;

[DefaultExecutionOrder(-1)]
[RequireComponent(typeof(TextMeshProUGUI))]
public class HUDTranslateText : MonoBehaviour
{
	[ValidateTranslation]
	public string TextId;

	public void Start()
	{
		if (!Globals.Initialized)
		{
			Globals.OnInitialized.AddListener(Start);
		}
		else
		{
			GetComponent<TMP_Text>().text = TextId?.tr();
		}
	}

	private void OnDestroy()
	{
		Globals.OnInitialized?.RemoveListener(Start);
	}

	private void UpdateTextInEditor()
	{
		TMP_Text textComponent = GetComponent<TMP_Text>();
		string translated;
		if (string.IsNullOrEmpty(TextId))
		{
			textComponent.text = "$EMPTY";
		}
		else if (LocalizationManager.CreateEditorOnlyTranslatorUncached().TryGetTranslation(TextId, out translated))
		{
			textComponent.text = "<color=#ff00f6>T</color> " + translated;
		}
		else
		{
			textComponent.text = "Not found: " + TextId;
		}
	}
}
