using Core.Dependency;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HUDIslandToolbarSlot : HUDBaseToolbarSlot
{
	[SerializeField]
	protected RectTransform UILayoutPreviewParent;

	private MetaIslandLayout _Layout;

	private Player Player;

	private ResearchManager ResearchManager;

	private ITutorialHighlightProvider TutorialHighlightProvider;

	protected bool Unlocked => Layout != null && ResearchManager.Progress.IsUnlocked(Layout);

	public MetaIslandLayout Layout
	{
		get
		{
			return _Layout;
		}
		set
		{
			if (!(value == _Layout))
			{
				_Layout = value;
				if (_Layout != null)
				{
					RenderLayoutPreview(Layout, UILayoutPreviewParent, 0.62f);
					base.TooltipText = _Layout.Title;
				}
				UpdateState();
				OnHighlightChanged();
			}
		}
	}

	[Construct]
	private void Construct(Player player, ResearchManager researchManager, ITutorialHighlightProvider tutorialHighlightProvider)
	{
		Player = player;
		ResearchManager = researchManager;
		TutorialHighlightProvider = tutorialHighlightProvider;
		Player.SelectedIslandLayout.Changed.AddListener(OnPlayerSelectedIslandLayoutChanged);
		ResearchManager.Progress.OnChanged.AddListener(UpdateState);
		TutorialHighlightProvider.HighlightChanged.AddListener(OnHighlightChanged);
	}

	protected override void OnDispose()
	{
		ResearchManager.Progress.OnChanged.RemoveListener(UpdateState);
		Player.SelectedIslandLayout.Changed.RemoveListener(OnPlayerSelectedIslandLayoutChanged);
		TutorialHighlightProvider.HighlightChanged.RemoveListener(OnHighlightChanged);
		base.OnDispose();
	}

	private void OnHighlightChanged()
	{
		base.Highlighted = Layout != null && TutorialHighlightProvider.IsIslandLayoutHighlighted(Layout);
	}

	protected void OnPlayerSelectedIslandLayoutChanged(MetaIslandLayout layout)
	{
		UpdateState();
	}

	protected void UpdateState()
	{
		if (Unlocked)
		{
			bool active = Player.SelectedIslandLayout.Value == Layout;
			SetState((!active) ? SlotState.Normal : SlotState.Selected);
		}
		else
		{
			SetState(SlotState.Locked);
		}
	}

	protected override void OnSlotClicked()
	{
		if (ResearchManager.Progress.IsUnlocked(Layout))
		{
			Player.SelectedIslandLayout.Value = Layout;
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	private static void RenderLayoutPreview(MetaIslandLayout baseLayout, RectTransform parent, float scale)
	{
		int uiMask = LayerMask.NameToLayer("UI");
		if (baseLayout.OverrideIconSprite != null)
		{
			GameObject iconObj = new GameObject("icon")
			{
				layer = uiMask
			};
			iconObj.transform.SetParent(parent.transform);
			Image image = iconObj.AddComponent<Image>();
			image.sprite = baseLayout.OverrideIconSprite;
			iconObj.transform.localPosition = Vector3.zero;
			iconObj.transform.localScale = Vector3.one * scale * 0.8f;
			image.color = HUDTheme.ColorIconButtonNormal;
			image.raycastTarget = false;
			image.material = Globals.Resources.DefaultUISpriteMaterial;
			return;
		}
		float baseImageSize = 100f;
		GameResources resources = Globals.Resources;
		EffectiveIslandLayout layout = baseLayout.LayoutsByRotation[0];
		float effectiveScale = math.clamp(1f / (float)math.max(layout.Dimensions.x, layout.Dimensions.y), 0.05f, 0.4f) * scale * 75f;
		int2 min_IC = (int2)baseLayout.Chunks[0].Tile_IC;
		int2 max_IC = min_IC;
		MetaIslandChunk[] chunks = layout.Chunks;
		foreach (MetaIslandChunk chunk in chunks)
		{
			min_IC = math.min(min_IC, (int2)chunk.Tile_IC);
			max_IC = math.max(max_IC, (int2)chunk.Tile_IC);
		}
		float2 tileOffset = new float2(0f, 0f) - 0.5f * (float2)(min_IC + max_IC);
		MetaIslandChunk[] chunks2 = layout.Chunks;
		foreach (MetaIslandChunk chunk2 in chunks2)
		{
			float2 chunkSpritePosition = ((int2)chunk2.Tile_IC + tileOffset) * effectiveScale;
			IslandChunkCoordinate tile_IC = chunk2.Tile_IC;
			GameObject chunkObj = new GameObject("tile-" + tile_IC.ToString(), typeof(RectTransform), typeof(Image))
			{
				layer = uiMask
			};
			Transform chunkTransform = chunkObj.transform;
			chunkTransform.SetParent(parent.transform);
			chunkTransform.localPosition = new Vector3(chunkSpritePosition.x, 0f - chunkSpritePosition.y, 0f);
			chunkTransform.localScale = new Vector3(effectiveScale, effectiveScale, effectiveScale) / baseImageSize * 0.9f;
			chunkTransform.localEulerAngles = new Vector3(0f, 0f, 0f);
			Image chunkSprite = chunkObj.GetComponent<Image>();
			chunkSprite.sprite = resources.UIIslandChunkPreview;
			chunkSprite.color = HUDTheme.ColorIconButtonNormal;
			chunkSprite.raycastTarget = false;
			chunkSprite.material = Globals.Resources.DefaultUISpriteMaterial;
		}
	}
}
