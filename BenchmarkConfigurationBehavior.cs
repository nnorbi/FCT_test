using TMPro;
using UnityEngine;

public class BenchmarkConfigurationBehavior : MonoBehaviour
{
	[SerializeField]
	private RectTransform UIInfoGrid;

	[SerializeField]
	private TMP_Text UIEntryTitle;

	[SerializeField]
	private TMP_Text UIBenchmarkInfoPrefab;

	public void Init(string benchmarkTitle, string[] additionalInfo)
	{
		UIEntryTitle.text = benchmarkTitle;
		UIInfoGrid.RemoveAllChildren();
		foreach (string info in additionalInfo)
		{
			TMP_Text textInstance = Object.Instantiate(UIBenchmarkInfoPrefab, UIInfoGrid);
			textInstance.text = info;
		}
	}
}
