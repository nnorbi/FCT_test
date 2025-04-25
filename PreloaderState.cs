using Core.Dependency;
using DG.Tweening;
using UnityEngine;

public abstract class PreloaderState : HUDComponent
{
	protected IPreloaderController PreloaderController;

	[Construct]
	private void Construct(IPreloaderController preloaderController)
	{
		PreloaderController = preloaderController;
	}

	protected abstract override void OnDispose();

	public virtual void OnFastForwardRequested()
	{
	}

	public virtual void OnEnterState()
	{
	}

	public virtual void OnLeaveState()
	{
	}

	protected void JoinFadeinToSequence(Sequence sequence, CanvasGroup element)
	{
		element.alpha = 0f;
		element.transform.localPosition = new Vector3(1800f, 0f, 500f);
		element.transform.localRotation = Quaternion.Euler(0f, 50f, 0f);
		sequence.Join(element.DOFade(1f, 3.5f));
		sequence.Join(element.transform.DOLocalMoveZ(0f, 3.5f).SetEase(Ease.OutExpo));
		sequence.Join(element.transform.DOLocalMoveX(0f, 1.5f).SetEase(Ease.OutExpo));
		sequence.Join(element.transform.DOLocalRotate(new Vector3(0f, 0f, 0f), 3.5f).SetEase(Ease.OutExpo));
	}

	protected void AppendFadeoutToSequence(Sequence sequence, CanvasGroup element)
	{
		sequence.Append(element.DOFade(0f, 0.4f));
		sequence.Join(element.transform.DOLocalMoveZ(500f, 0.5f).SetEase(Ease.InExpo));
		sequence.Join(element.transform.DOLocalMoveX(-1800f, 0.5f).SetEase(Ease.InExpo));
		sequence.Join(element.transform.DOLocalRotate(new Vector3(0f, -50f, 0f), 0.5f).SetEase(Ease.InExpo));
	}
}
