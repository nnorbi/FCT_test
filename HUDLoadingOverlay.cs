using Core.Dependency;
using Unity.Core.View;

public class HUDLoadingOverlay : HUDPart, IRunnableView, IView
{
	protected bool Loading = true;

	public override bool NeedsGraphicsRaycaster => false;

	public void Run()
	{
		Invoke("FinishLoading", 0.5f);
	}

	[Construct]
	public void Construct()
	{
		Events.ShowLoadingOverlay.AddListener(OnShowLoadingOverlay);
	}

	protected override void OnDispose()
	{
		Events.ShowLoadingOverlay.RemoveListener(OnShowLoadingOverlay);
	}

	private void OnShowLoadingOverlay()
	{
		Loading = true;
		base.gameObject.SetActive(value: true);
	}

	protected void FinishLoading()
	{
		Loading = false;
		base.gameObject.SetActive(value: false);
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (Loading)
		{
			context.ConsumeAll();
		}
	}
}
