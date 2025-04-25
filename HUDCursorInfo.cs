using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HUDCursorInfo : MonoBehaviour
{
	public class Data
	{
		public Severity Severity;

		public string Text;

		public static void Merge(ref Data result, Severity severity, string text)
		{
			if (result == null || severity > result.Severity)
			{
				result = new Data
				{
					Severity = severity,
					Text = text
				};
			}
		}
	}

	public enum Severity
	{
		Fatal = 150,
		Error = 100,
		Warning = 50,
		Info = 25
	}

	public Sprite UIErrorIcon;

	public Sprite UIWarningIcon;

	public Sprite UIInfoIcon;

	public Image UIIcon;

	public TMP_Text UIText;

	public EditorDict<Severity, Sprite> UIIcons;

	public EditorDict<Severity, Color> UIIconColors;

	protected Severity CurrentSeverity = (Severity)0;

	protected string CurrentText = "";

	public void SetDataAndUpdate(Data info, Player player)
	{
		if (info == null)
		{
			CurrentSeverity = (Severity)0;
			CurrentText = null;
			base.gameObject.SetActiveSelfExt(active: false);
			return;
		}
		Vector3 mousePosition = Singleton<GameCore>.G.DrawSceneReferences.UICamera.ScreenToWorldPoint(new float3(player.Viewport.CursorScreenPosition, 0f));
		mousePosition.z = base.transform.position.z;
		base.transform.position = mousePosition;
		if (!base.gameObject.activeSelf || info.Severity != CurrentSeverity || !(info.Text == CurrentText))
		{
			CurrentSeverity = info.Severity;
			CurrentText = info.Text;
			UIIcon.sprite = UIIcons.Get(info.Severity);
			UIText.text = info.Text;
			Color color = UIIconColors.Get(info.Severity);
			UIText.color = color;
			UIIcon.color = color;
			if (!base.gameObject.activeSelf)
			{
				DOTween.Kill(base.transform, complete: true);
				base.transform.localScale = new float3(0.8f, 0.9f, 1f);
				base.transform.DOScale(new float3(1f, 1f, 1f), 0.8f).SetEase(Ease.OutElastic);
			}
			base.gameObject.SetActiveSelfExt(active: true);
		}
	}
}
