using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BenchmarkEntryBehavior : MonoBehaviour
{
	[SerializeField]
	private Button UIPlayButton;

	[SerializeField]
	private BenchmarkConfigurationBehavior UIBenchmarkConfigurationPrefab;

	[SerializeField]
	private RectTransform UIConfigurationsParent;

	[SerializeField]
	private TMP_Text UIEntryTitle;

	[SerializeField]
	private TMP_Text UIEntryDescription;

	public void Init(string benchmarkTitle, string benchmarkDescription, UnityAction onRunBenchmark)
	{
		UIEntryTitle.text = benchmarkTitle;
		UIEntryDescription.text = benchmarkDescription;
		UIConfigurationsParent.RemoveAllChildren();
		HUDTheme.PrepareTheme(UIPlayButton, HUDTheme.ButtonColorsActive, animateOnClick: true, clickSounds: true, disableNavigation: false).onClick.AddListener(onRunBenchmark);
	}

	public void AddConfiguration(string title, string[] settings)
	{
		BenchmarkConfigurationBehavior configInstance = Object.Instantiate(UIBenchmarkConfigurationPrefab, UIConfigurationsParent);
		configInstance.Init(title, settings);
	}
}
