using UnityEngine;
using UnityEngine.UI;

public class HUDAdjustCanvasScaler : MonoBehaviour
{
	protected CanvasScaler Scaler;

	private EnumGameSettingLegacy<float> UIScaleSetting => Globals.Settings.Interface.UIScale;

	public void Start()
	{
		Scaler = GetComponent<CanvasScaler>();
		UpdateScale();
		UIScaleSetting.Changed.AddListener(UpdateScale);
	}

	public void OnDestroy()
	{
		UIScaleSetting.Changed.RemoveListener(UpdateScale);
	}

	protected void UpdateScale()
	{
		float scale = UIScaleSetting.Current.Value;
		Scaler.referenceResolution = new Vector2(1920f, 1080f) / scale;
	}
}
