using Core.Dependency;
using DG.Tweening;
using UnityEngine;

public class HUDAutosave : HUDPart
{
	public static string TOKEN_AUTOSAVE_AVAILABLE = "HUDAutosave$autosave_available";

	[SerializeField]
	private Transform UISaveIndicatorIcon;

	private Sequence AutosaveSequence;

	private float LastAutosave = -1E+10f;

	private SavegameCoordinator SavegameCoordinator;

	public override bool NeedsGraphicsRaycaster => false;

	[Construct]
	private void Construct(SavegameCoordinator savegameCoordinator)
	{
		SavegameCoordinator = savegameCoordinator;
		UISaveIndicatorIcon.localScale = new Vector3(0f, 0f, 0f);
		UISaveIndicatorIcon.transform.localPosition = new Vector3(0f, 100f, 0f);
	}

	protected override void OnDispose()
	{
		AutosaveSequence.Kill();
	}

	private void StartSaveSequence()
	{
		if (AutosaveSequence != null)
		{
			Debug.LogWarning("Not starting double autosave");
			return;
		}
		LastAutosave = Time.realtimeSinceStartup;
		AutosaveSequence = DOTween.Sequence();
		UISaveIndicatorIcon.localScale = new Vector3(0f, 0f, 0f);
		AutosaveSequence.Join(UISaveIndicatorIcon.DOScale(1f, 0.9f).SetEase(Ease.OutBounce));
		AutosaveSequence.Join(UISaveIndicatorIcon.DOLocalMoveY(0f, 0.9f).SetEase(Ease.OutBounce));
		AutosaveSequence.Append(UISaveIndicatorIcon.DOScale(2f, 1.5f).SetEase(Ease.InOutCubic));
		AutosaveSequence.AppendCallback(delegate
		{
			SavegameCoordinator.SaveCurrentSync();
			Debug.Log("HUDAutosave:: Autosave finished");
		});
		AutosaveSequence.Append(UISaveIndicatorIcon.DOScale(0f, 1f).SetEase(Ease.InBack));
		AutosaveSequence.OnComplete(delegate
		{
			AutosaveSequence = null;
		});
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (context.ConsumeToken(TOKEN_AUTOSAVE_AVAILABLE))
		{
			float autosaveIntervalSeconds = Globals.Settings.General.AutosaveInterval.Current.Value * 60f;
			float now = Time.realtimeSinceStartup;
			if (autosaveIntervalSeconds > 0f && now - LastAutosave > autosaveIntervalSeconds)
			{
				StartSaveSequence();
			}
		}
	}
}
