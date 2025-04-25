using System.Collections.Generic;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDRenderStats : HUDPart, IRunnableView, IView
{
	private const string PLAYER_PREFS_KEY = "render-stats-visible";

	[SerializeField]
	private RectTransform UIRenderBucketsParent;

	[SerializeField]
	private PrefabViewReference<HUDRenderBucketStat> UIRenderBucketPrefab;

	private DrawManager DrawManager;

	public override bool ShouldInitialize => Application.isEditor;

	public void Run()
	{
		UIRenderBucketsParent.RemoveAllChildren();
		foreach (KeyValuePair<RenderCategory, RenderCategoryBucket> entry in DrawManager.DrawOptions.RenderStats.Buckets)
		{
			HUDRenderBucketStat bucket = RequestChildView(UIRenderBucketPrefab).PlaceAt(UIRenderBucketsParent);
			bucket.Bucket = entry.Value;
		}
	}

	[Construct]
	private void Construct(DebugConsole debugConsole, DrawManager drawManager)
	{
		DrawManager = drawManager;
		base.gameObject.SetActive(PlayerPrefs.GetInt("render-stats-visible", 0) != 0);
		if (ShouldInitialize)
		{
			debugConsole.Register("debug.toggle-render-stats", delegate
			{
				base.gameObject.SetActiveSelfExt(!base.gameObject.activeSelf);
				PlayerPrefs.SetInt("render-stats-visible", base.gameObject.activeSelf ? 1 : 0);
			});
		}
	}

	protected override void OnDispose()
	{
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (base.gameObject.activeSelf)
		{
			base.OnGameUpdate(context, drawOptions);
		}
	}
}
