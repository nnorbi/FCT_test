using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class LayerManager
{
	protected float InterpolatedLayer = 0f;

	protected Sequence CurrentAnimation = null;

	protected Texture2D LayerTexture = null;

	protected Texture2D BeltArrowTexture = null;

	protected Player Player;

	public LayerManager()
	{
		Singleton<GameCore>.G.OnGameInitialized.AddListener(Initialize);
	}

	protected void Initialize()
	{
		Player = Singleton<GameCore>.G.LocalPlayer;
		InterpolatedLayer = Player.Viewport.Layer;
		SetShaderParams();
		GenerateLayerTexture();
		GenerateBeltArrowTexture();
		Player.MapChanged.AddListener(OnMaxLayerPotentiallyChanged);
		Singleton<GameCore>.G.Research.Progress.OnChanged.AddListener(delegate
		{
			OnMaxLayerPotentiallyChanged(Player.CurrentMap);
		});
		Player.Viewport.ShowAllLayersChanged.AddListener(SetShaderParams);
		Player.Viewport.LayerChanged.AddListener(OnLayerChanged);
		Player.Viewport.NewViewportLoaded.AddListener(OnViewportLoaded);
	}

	protected void OnViewportLoaded()
	{
		CurrentAnimation?.Kill();
		CurrentAnimation = null;
		InterpolatedLayer = Player.Viewport.Layer;
		SetShaderParams();
	}

	protected void OnLayerChanged()
	{
		CurrentAnimation?.Kill();
		CurrentAnimation = DOTween.Sequence();
		bool goDown = (float)Player.Viewport.Layer < InterpolatedLayer;
		CurrentAnimation.Join(DOTween.To(() => InterpolatedLayer, delegate(float x)
		{
			InterpolatedLayer = x;
			SetShaderParams();
		}, Player.Viewport.Layer, goDown ? 0.22f : 0.3f).SetEase(goDown ? Ease.OutQuad : Ease.OutQuint));
	}

	protected void OnMaxLayerPotentiallyChanged(GameMap map)
	{
		short maxLayer = map.InteractionMode.GetMaximumAllowedLayer(Player);
		if (Player.Viewport.Layer > maxLayer)
		{
			SwitchLayer(maxLayer);
		}
		if (maxLayer == 0 && !Player.Viewport.ShowAllLayers)
		{
			Player.Viewport.ShowAllLayers = true;
		}
	}

	public void SwitchLayer(short newLayer)
	{
		if (newLayer < 0 || newLayer > Player.CurrentMap.InteractionMode.GetMaximumAllowedLayer(Player))
		{
			Debug.LogWarning("Invalid layer: " + newLayer);
			return;
		}
		Player.Viewport.Layer = newLayer;
		if (Application.isEditor)
		{
			GenerateLayerTexture();
		}
	}

	protected void SetShaderParams()
	{
		float layer = InterpolatedLayer;
		GameResources resources = Globals.Resources;
		Color layerColor = resources.LayerColors.LerpArray(layer);
		float4 @float = new float4(0);
		@float.x = layer + 1f;
		@float.y = layer + (float)Singleton<GameCore>.G.Mode.MaxLayer + 1f;
		@float.z = (Player.Viewport.ShowAllLayers ? 1f : 0f);
		@float.w = layer;
		float4 layerParams = @float;
		Shader.SetGlobalColor(GlobalShaderInputs.CurrentLayerColor, layerColor);
		Shader.SetGlobalVector(GlobalShaderInputs.LayersParams, layerParams);
	}

	protected void GenerateLayerTexture()
	{
		GameResources resources = Globals.Resources;
		Color[] colors = resources.LayerColors;
		if (LayerTexture == null)
		{
			LayerTexture = new Texture2D(1, colors.Length, TextureFormat.RGBA32, 0, linear: false);
		}
		LayerTexture.filterMode = FilterMode.Bilinear;
		LayerTexture.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < colors.Length; i++)
		{
			LayerTexture.SetPixel(0, i, colors[i]);
		}
		LayerTexture.Apply();
		Shader.SetGlobalTexture(GlobalShaderInputs.LayerColorTexture, LayerTexture);
		Shader.SetGlobalFloat(GlobalShaderInputs.LayerCount, colors.Length);
	}

	protected void GenerateBeltArrowTexture()
	{
		GameResources resources = Globals.Resources;
		Color[] colors = resources.BeltArrowColors;
		if (BeltArrowTexture == null)
		{
			BeltArrowTexture = new Texture2D(1, colors.Length, TextureFormat.RGBA32, 0, linear: false);
		}
		BeltArrowTexture.filterMode = FilterMode.Bilinear;
		BeltArrowTexture.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < colors.Length; i++)
		{
			BeltArrowTexture.SetPixel(0, i, colors[i]);
		}
		BeltArrowTexture.Apply();
		Shader.SetGlobalTexture(GlobalShaderInputs.BeltArrowColorTexture, BeltArrowTexture);
	}

	public void SyncViewportHeight()
	{
		Player.Viewport.Height = InterpolatedLayer;
	}
}
