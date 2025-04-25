using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HUDProgressBar : MonoBehaviour
{
	[SerializeField]
	protected Image UIProgressImage;

	public void SetProgress(float progress)
	{
		UIProgressImage.fillAmount = math.clamp(progress, 0f, 1f);
	}
}
