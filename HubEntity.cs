using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class HubEntity : MapEntity<MetaBuildingInternalVariant>
{
	[Serializable]
	public class AnimationParameters
	{
		public float Gravity = -70f;

		public float VortexForceUp = 50f;

		public float VortexForceUpVarying = 5f;

		public float VortexForceUpVaryingTimeScale = 2f;

		public float ForceTowardsCenter = 5f;

		public float ForceTowardsCenterIncrease = 0.2f;

		public float RadialForceStrength = 5.4f;

		public float BaseRadialSpeed = 10f;

		public float AnimationSpeed = 1f;

		public float VortexRadius = 6f;

		public float RotationXVelocity = 45f;

		public float RotationYLerpFactor = 22f;

		public float SpeedupTimeFactor = 3f;

		public float SpeedupMax = 100f;

		public float InitialVelocityXY = 30f;

		public float InitialVelocityZ = 17f;

		public float InitialVelocityImmutableFor = 0.3f;

		public float MaxDepthBeforeDestroy = -30f;
	}

	[Serializable]
	protected class OngoingAnimation
	{
		[SerializeReference]
		public BeltItem Item;

		public float TimeElapsed = 0f;

		public float3 Velocity_I;

		public float3 Pos_I;

		public float RotationY = 0f;

		public float RotationX = 0f;

		public bool AddedToStorage = false;
	}

	public class InputSlot
	{
		public IslandTileCoordinate Tile_I;

		public Grid.Direction Direction_G;

		public double LastValidItemTime_G = -10000000000.0;

		public double LastInvalidItemTime_G = -10000000000.0;

		public double LastOutdatedItemTime_G = -10000000000.0;
	}

	public static int INSTANCING_ID_HUB_SPOT_LOCKED = Shader.PropertyToID("hub-spot-indicator::locked-slot");

	protected static int INSTANCING_ID_HUB_SPOT_INDICATOR_UNUSED = Shader.PropertyToID("hub-spot-indicator::unused");

	protected static int INSTANCING_ID_HUB_SPOT_INDICATOR_VALID = Shader.PropertyToID("hub-spot-indicator::valid");

	protected static int INSTANCING_ID_HUB_SPOT_INDICATOR_INVALID = Shader.PropertyToID("hub-spot-indicator::invalid");

	protected static int INSTANCING_ID_HUB_SPOT_INDICATOR_OUTDATED = Shader.PropertyToID("hub-spot-indicator::outdated");

	protected static float ADD_ITEM_TO_STORAGE_AFTER_SECONDS = 0.5f;

	protected int InputCount = 4;

	protected int MaxInputCount = 14;

	protected float CurrentGlowAlpha = 0f;

	public List<InputSlot> InputSlots;

	public List<InputSlot> UnavailableInputSlots;

	protected Dictionary<IslandTileCoordinate, InputSlot> Tile_I_ToSlot = new Dictionary<IslandTileCoordinate, InputSlot>();

	protected List<OngoingAnimation> Animations = new List<OngoingAnimation>();

	private static void UpdateLastItemTime(BeltItem item, InputSlot slot)
	{
		ref double itemTime = ref slot.LastInvalidItemTime_G;
		if (item is ShapeItem shapeItem)
		{
			string shape = shapeItem.Definition.Hash;
			if (Singleton<GameCore>.G.Research.ShapeManager.IsResearchShape(shape))
			{
				itemTime = ref slot.LastValidItemTime_G;
			}
			else if (Singleton<GameCore>.G.Research.ShapeManager.IsCurrencyGrantingShape(shape))
			{
				itemTime = ref slot.LastOutdatedItemTime_G;
			}
		}
		itemTime = Singleton<GameCore>.G.SimulationSpeed.SimulationTime_G;
	}

	public HubEntity(CtorArgs payload)
		: base(payload)
	{
		ComputeInputSlots();
		Singleton<GameCore>.G.Research.Progress.OnChanged.AddListener(ComputeInputSlots);
	}

	protected void ComputeInputSlots()
	{
		ResearchProgress progress = Singleton<GameCore>.G.Research.Progress;
		int inputCount = Singleton<GameCore>.G.Mode.ResearchConfig.HUBInitialSize;
		int maxInputCount = inputCount;
		foreach (KeyValuePair<MetaResearchable, int> entry in Singleton<GameCore>.G.Mode.ResearchConfig.HUBSizeUnlocks)
		{
			if (progress.IsUnlocked(entry.Key))
			{
				inputCount += entry.Value;
			}
			maxInputCount += entry.Value;
		}
		InputCount = inputCount;
		MaxInputCount = maxInputCount;
		InputSlots = new List<InputSlot>();
		UnavailableInputSlots = new List<InputSlot>();
		Tile_I_ToSlot = new Dictionary<IslandTileCoordinate, InputSlot>();
		short maxLayer = Singleton<GameCore>.G.Mode.MaxLayer;
		float centerIndex = (float)(MaxInputCount / 2) - 0.5f;
		for (short layer = 0; layer <= maxLayer; layer++)
		{
			for (int side = 0; side < 4; side++)
			{
				Grid.Direction direction = (Grid.Direction)side;
				Grid.Direction slotDirection = Grid.OppositeDirection(direction);
				for (int i = 0; i < MaxInputCount; i++)
				{
					IslandTileCoordinate tile_I = ComputeInputTilePosition_I(direction, i, layer);
					InputSlot slot = new InputSlot
					{
						Tile_I = tile_I,
						Direction_G = Grid.RotateDirection(slotDirection, Rotation_G)
					};
					if (math.abs((float)i - centerIndex) < (float)(InputCount / 2))
					{
						InputSlots.Add(slot);
						Tile_I_ToSlot[slot.Tile_I] = slot;
					}
					else
					{
						UnavailableInputSlots.Add(slot);
					}
				}
			}
		}
	}

	protected IslandTileCoordinate ComputeInputTilePosition_I(Grid.Direction side, int slotIndex, short layer)
	{
		int offset = slotIndex - MaxInputCount / 2;
		if (1 == 0)
		{
		}
		TileDirection tileDirection = side switch
		{
			Grid.Direction.Right => new TileDirection(9, offset, layer), 
			Grid.Direction.Bottom => new TileDirection(offset, 9, layer), 
			Grid.Direction.Left => new TileDirection(-10, offset, layer), 
			Grid.Direction.Top => new TileDirection(offset, -10, layer), 
			_ => new TileDirection(0, 0, 0), 
		};
		if (1 == 0)
		{
		}
		TileDirection tile_L = tileDirection;
		return Tile_I + tile_L.Rotate(Rotation_G);
	}

	public bool IsValidBeltPortInput(Island island, IslandTileCoordinate sourceTile_I, Grid.Direction rotation_G)
	{
		if (island != Island)
		{
			return false;
		}
		if (Tile_I_ToSlot.TryGetValue(sourceTile_I, out var slot))
		{
			return slot.Direction_G == rotation_G;
		}
		return false;
	}

	public void AcceptItemFromBeltPort(BeltItem item, int remainingTicks_T, IslandTileCoordinate tile_I, Grid.Direction sourceRotation_G)
	{
		AnimationParameters animParams = Globals.Resources.HUBAnimationParameters;
		Animations.Add(new OngoingAnimation
		{
			Item = item,
			TimeElapsed = (float)((double)remainingTicks_T * IslandSimulator.SECONDS_PER_TICK),
			Velocity_I = new float3(Grid.Rotate(new float2(animParams.InitialVelocityXY, 0f), sourceRotation_G), animParams.InitialVelocityZ),
			Pos_I = new float3((int3)tile_I + new float3(0f, 0f, 0.25f))
		});
		InputSlot slot = Tile_I_ToSlot[tile_I];
		UpdateLastItemTime(item, slot);
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		float delta = math.min(options.DeltaTime, 0.1f);
		float3 center_I = I_From_L(new float3(-0.5f, -0.5f, 0.5f));
		AnimationParameters animParams = Globals.Resources.HUBAnimationParameters;
		float timeFloat = (float)(options.SimulationTime_G % 2048.0);
		float vortexForceUp = animParams.VortexForceUp + animParams.VortexForceUpVarying * math.sin(timeFloat * animParams.VortexForceUpVaryingTimeScale);
		int maxAnimationCount = InputCount * 4 * 5 * Singleton<GameCore>.G.Mode.MaxLayer;
		float targetGlowAlpha = math.pow(math.saturate((float)Animations.Count / (float)maxAnimationCount), 0.25f);
		CurrentGlowAlpha = math.lerp(CurrentGlowAlpha, targetGlowAlpha, math.saturate(15f * options.DeltaTime));
		for (int i = 0; i < Animations.Count; i++)
		{
			OngoingAnimation animation = Animations[i];
			animation.TimeElapsed += delta;
			if (!animation.AddedToStorage && animation.TimeElapsed > ADD_ITEM_TO_STORAGE_AFTER_SECONDS)
			{
				if (animation.Item is ShapeItem shapeItem)
				{
					Singleton<GameCore>.G.Research.ShapeStorage.Add(shapeItem.Definition, 1);
				}
				animation.AddedToStorage = true;
			}
			if (animation.TimeElapsed > 100f)
			{
				Debug.LogError("Hub item anim > 100f seconds; manually deleting.");
				Animations.DeleteBySwappingWithLast_ForwardIteration(ref i);
				continue;
			}
			if (animation.Pos_I.z < animParams.MaxDepthBeforeDestroy)
			{
				if (!animation.AddedToStorage)
				{
					Debug.LogWarning("Item not added to storage yet, at t=" + animation.TimeElapsed);
					if (animation.Item is ShapeItem shapeItem2)
					{
						Singleton<GameCore>.G.Research.ShapeStorage.Add(shapeItem2.Definition, 1);
					}
				}
				Animations.DeleteBySwappingWithLast_ForwardIteration(ref i);
				continue;
			}
			float3 vectorToCenter_I = center_I - animation.Pos_I;
			float distanceToCenter_XY = math.length(vectorToCenter_I.xy);
			float2 directionToCenter_I_XY = vectorToCenter_I.xy / distanceToCenter_XY;
			if (animation.TimeElapsed > animParams.InitialVelocityImmutableFor)
			{
				float speedFactor = math.clamp(1f + animation.TimeElapsed / animParams.SpeedupTimeFactor, 0f, animParams.SpeedupMax);
				float radialSpeed = animParams.BaseRadialSpeed * speedFactor;
				animation.Velocity_I = math.lerp(y: new float3(directionToCenter_I_XY.y * radialSpeed, (0f - directionToCenter_I_XY.x) * radialSpeed, animation.Velocity_I.z), x: animation.Velocity_I, s: math.saturate(animParams.RadialForceStrength * delta));
				float forceStrength = animParams.ForceTowardsCenter * distanceToCenter_XY * speedFactor * (1f + math.max(0f, 0f - animation.Pos_I.z) * animParams.ForceTowardsCenterIncrease);
				animation.Velocity_I += new float3(directionToCenter_I_XY * delta * forceStrength, 0f);
				animation.Velocity_I += new float3(0f, 0f, animParams.Gravity * delta);
				if (distanceToCenter_XY < animParams.VortexRadius)
				{
					animation.Velocity_I += new float3(0f, 0f, vortexForceUp * delta);
					float angle = math.degrees(math.atan2(vectorToCenter_I.y, vectorToCenter_I.x));
					animation.RotationX -= animParams.RotationXVelocity * delta;
					animation.RotationY = Mathf.LerpAngle(animation.RotationY, angle, math.saturate(animParams.RotationYLerpFactor * delta));
				}
			}
			animation.Pos_I += animation.Velocity_I * delta * animParams.AnimationSpeed;
		}
	}

	public override Drawing_CullMode Order_GetCullMode()
	{
		return Drawing_CullMode.DrawAlways_NEEDS_MANUAL_CULLING;
	}

	protected override void DrawStatic_BaseMesh(MeshBuilder builder)
	{
	}

	private void Draw_LockedSpotsIndicators(FrameDrawOptions drawOptions)
	{
		foreach (InputSlot slot in UnavailableInputSlots)
		{
			if (slot.Tile_I.z == drawOptions.Player.Viewport.Layer)
			{
				float3 translation = slot.Tile_I.To_W(Island);
				translation.y = drawOptions.Viewport.Height + 0.01f;
				drawOptions.Draw3DPlaneWithMaterialInstanced(INSTANCING_ID_HUB_SPOT_LOCKED, drawOptions.Theme.BaseResources.UXHubSpotLockedMaterial, FastMatrix.TranslateRotate(in translation, Grid.OppositeDirection(slot.Direction_G)));
			}
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		float3 pos_W = W_From_L(new float3(0f, 0f, 0f));
		if (!GeometryUtility.TestPlanesAABB(options.CameraPlanes, new Bounds(pos_W, new Vector3(25f, 25f, 100f))))
		{
			return;
		}
		Draw_LockedSpotsIndicators(options);
		VisualThemeBaseResources themeResources = options.Theme.BaseResources;
		if (InternalVariant.SupportMeshesInternalLOD[0].TryGet(0, out LODBaseMesh.CachedMesh glowMesh))
		{
			options.RegularRenderer.DrawMesh(glowMesh, material: themeResources.HUBGlowMaterial, matrix: FastMatrix.Translate(new float3(-10.5f, -0.79f, -9.5f)), category: RenderCategory.Misc, properties: MaterialPropertyHelpers.CreateAlphaBlock(CurrentGlowAlpha));
		}
		for (int i = 0; i < Animations.Count; i++)
		{
			OngoingAnimation animation = Animations[i];
			BeltItem item = animation.Item;
			options.ShapeInstanceManager.AddInstance(item.GetDefaultInstancingKey(), item.GetMesh(), item.GetMaterial(), Matrix4x4.TRS(Island.W_From_I(in animation.Pos_I), Quaternion.Euler(0f, animation.RotationY, animation.RotationX), Vector3.one));
		}
		double now = options.SimulationTime_G;
		foreach (InputSlot slot in InputSlots)
		{
			MapEntity contents = Island.GetEntity_I(in slot.Tile_I);
			if (contents is BeltPortSenderEntity)
			{
				bool outdated = now - slot.LastOutdatedItemTime_G < 5.0;
				bool active = now - slot.LastValidItemTime_G < 5.0;
				Material material;
				int instancingKey;
				if (now - slot.LastInvalidItemTime_G < 5.0)
				{
					material = themeResources.UXHubSpotIndicatorInvalidMaterial;
					instancingKey = INSTANCING_ID_HUB_SPOT_INDICATOR_INVALID;
				}
				else if (active)
				{
					material = themeResources.UXHubSpotIndicatorValidMaterial;
					instancingKey = INSTANCING_ID_HUB_SPOT_INDICATOR_VALID;
				}
				else if (outdated)
				{
					material = themeResources.UXHubSpotIndicatorOutdatedMaterial;
					instancingKey = INSTANCING_ID_HUB_SPOT_INDICATOR_OUTDATED;
				}
				else
				{
					material = themeResources.UXHubSpotIndicatorUnusedMaterial;
					instancingKey = INSTANCING_ID_HUB_SPOT_INDICATOR_UNUSED;
				}
				options.Draw3DPlaneWithMaterialInstanced(instancingKey, material, FastMatrix.TranslateRotate(Island.W_From_I((int3)slot.Tile_I + new float3(Grid.Rotate(new float2(1.2f, 0f), slot.Direction_G), 0.3f)), slot.Direction_G));
			}
		}
	}
}
