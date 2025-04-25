using System;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class DebugViewColliders : IDebugView
{
	private enum BoundingBoxFace
	{
		Left,
		Right,
		Back,
		Front,
		Bottom,
		Top
	}

	private class ColliderHandle
	{
		private float3 CursorOffset;

		private float3 HandlePosition_W;

		private readonly MetaBuildingInternalVariant.CollisionBox Collider;

		private readonly MapEntity Entity;

		private Bounds Bounds_W;

		private readonly float3 BoundsMin_L;

		private readonly float3 BoundsMax_L;

		private readonly BoundingBoxFace Face;

		public ColliderHandle(float3 handlePosition_W, MetaBuildingInternalVariant.CollisionBox collider, MapEntity entity, BoundingBoxFace face)
		{
			HandlePosition_W = handlePosition_W;
			Collider = collider;
			Entity = entity;
			Face = face;
			float3 center_W = entity.W_From_L(in collider.Center_L);
			float3 dimensions_W = collider.DimensionsByRotation_W[(int)entity.Rotation_G];
			Bounds_W = new Bounds(center_W, dimensions_W);
			Camera camera = Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera;
			Vector3 inputMousePosition = Input.mousePosition;
			inputMousePosition.z = camera.WorldToScreenPoint(HandlePosition_W).z;
			CursorOffset = handlePosition_W - (float3)camera.ScreenToWorldPoint(inputMousePosition);
			int3 min_L = (int3)Entity.InternalVariant.Tiles[0];
			int3 max_L = (int3)Entity.InternalVariant.Tiles[0];
			for (int i = 1; i < Entity.InternalVariant.Tiles.Length; i++)
			{
				min_L = math.min(min_L, (int3)Entity.InternalVariant.Tiles[i]);
				max_L = math.max(max_L, (int3)Entity.InternalVariant.Tiles[i]);
			}
			BoundsMin_L = min_L + new float3(-0.5f, -0.5f, 0f);
			BoundsMax_L = max_L + new float3(0.5f, 0.5f, 1f);
		}

		public void UpdateWorldPosition()
		{
			Camera camera = Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera;
			camera.WorldToScreenPoint(HandlePosition_W);
			Vector3 inputMousePosition = Input.mousePosition;
			inputMousePosition.z = camera.WorldToScreenPoint(HandlePosition_W).z;
			float3 position_W = (float3)camera.ScreenToWorldPoint(inputMousePosition) + CursorOffset;
			Vector3 boundsMin = Bounds_W.min;
			Vector3 boundsMax = Bounds_W.max;
			switch (Face)
			{
			case BoundingBoxFace.Left:
				boundsMin.x = position_W.x;
				break;
			case BoundingBoxFace.Right:
				boundsMax.x = position_W.x;
				break;
			case BoundingBoxFace.Bottom:
				boundsMin.y = position_W.y;
				break;
			case BoundingBoxFace.Top:
				boundsMax.y = position_W.y;
				break;
			case BoundingBoxFace.Back:
				boundsMin.z = position_W.z;
				break;
			case BoundingBoxFace.Front:
				boundsMax.z = position_W.z;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			Bounds_W.min = boundsMin;
			Bounds_W.max = boundsMax;
			ConvertWorldBoundsToLocal();
			Collider.ComputeDimensions();
		}

		private void ConvertWorldBoundsToLocal()
		{
			float3 dimensions_L = Grid.G_From_W(Bounds_W.size);
			dimensions_L.xy = math.abs(Grid.RotateInverse(dimensions_L.xy, Entity.Rotation_G));
			float3 center_L = Grid.G_From_W((Vector3)((float3)Bounds_W.center - Entity.W_From_L((float3)int3.zero)));
			center_L.xy = Grid.RotateInverse(center_L.xy, Entity.Rotation_G);
			float3 min = center_L - dimensions_L * 0.5f;
			float3 max = center_L + dimensions_L * 0.5f;
			min = math.max(min, BoundsMin_L);
			max = math.min(max, BoundsMax_L);
			Collider.Dimensions_L = max - min;
			Collider.Center_L = (min + max) * 0.5f;
		}
	}

	public const string ID = "colliders";

	private readonly float3 HandleDrawnSize = 0.04f;

	private readonly float3 HandlePickSize = 0.065f;

	private bool StartedDragging;

	private bool Dragging;

	private bool EndingDrag;

	private ColliderHandle DraggedColliderHandle;

	public string Name => "Colliders";

	private static float3 GetHandlePosition(float3 center, float3 dimensions, BoundingBoxFace face)
	{
		float3 min = center - dimensions * 0.5f;
		float3 max = center + dimensions * 0.5f;
		if (1 == 0)
		{
		}
		float3 result = face switch
		{
			BoundingBoxFace.Left => new float3(min.x, center.yz), 
			BoundingBoxFace.Right => new float3(max.x, center.yz), 
			BoundingBoxFace.Bottom => new float3(center.x, min.y, center.z), 
			BoundingBoxFace.Top => new float3(center.x, max.y, center.z), 
			BoundingBoxFace.Back => new float3(center.xy, min.z), 
			BoundingBoxFace.Front => new float3(center.xy, max.z), 
			_ => throw new ArgumentOutOfRangeException("face", face, null), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public void HandleInput(InputDownstreamContext inputs)
	{
		bool currentlyDragging = Input.GetKey(KeyCode.Mouse0);
		StartedDragging = !Dragging && currentlyDragging;
		EndingDrag = Dragging && !currentlyDragging;
		Dragging = currentlyDragging;
		if (StartedDragging)
		{
			TryGetHandle();
		}
		if (EndingDrag)
		{
			DraggedColliderHandle = null;
		}
		if (Dragging && DraggedColliderHandle != null)
		{
			inputs.ConsumeAll();
			DraggedColliderHandle.UpdateWorldPosition();
		}
	}

	public void OnGameDraw()
	{
		CommandBuilder draw = DrawingManager.GetBuilder(renderInGame: true);
		Camera camera = Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera;
		draw.cameraTargets = new Camera[1] { camera };
		foreach (Island island in Singleton<GameCore>.G.LocalPlayer.CurrentMap.Islands)
		{
			foreach (MapEntity building in island.Buildings.Buildings)
			{
				MetaBuildingInternalVariant.CollisionBox[] colliders = building.InternalVariant.Colliders;
				foreach (MetaBuildingInternalVariant.CollisionBox collider in colliders)
				{
					float3 center_W = building.W_From_L(in collider.Center_L);
					float3 dimensions_W = collider.DimensionsByRotation_W[(int)building.Rotation_G];
					draw.WireBox(center_W, dimensions_W, new Color(0f, 1f, 0f, 0.8f));
					draw.SolidBox(center_W, dimensions_W, new Color(0f, 1f, 0f, 0.1f));
					DrawHandles(building, collider, draw);
				}
			}
		}
		draw.Dispose();
	}

	private void TryGetHandle()
	{
		Camera camera = Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera;
		Ray ray = camera.ScreenPointToRay(Input.mousePosition);
		DraggedColliderHandle = null;
		foreach (Island island in Singleton<GameCore>.G.LocalPlayer.CurrentMap.Islands)
		{
			foreach (MapEntity building in island.Buildings.Buildings)
			{
				MetaBuildingInternalVariant.CollisionBox[] colliders = building.InternalVariant.Colliders;
				foreach (MetaBuildingInternalVariant.CollisionBox collider in colliders)
				{
					DraggedColliderHandle = FindIntersectedHandle(ray, building, collider);
					if (DraggedColliderHandle != null)
					{
						return;
					}
				}
			}
		}
	}

	private unsafe ColliderHandle FindIntersectedHandle(Ray ray, MapEntity building, MetaBuildingInternalVariant.CollisionBox collider)
	{
		float3 center_W = building.W_From_L(in collider.Center_L);
		float3 dimensions_W = collider.DimensionsByRotation_W[(int)building.Rotation_G];
		byte* num = stackalloc byte[24];
		*(int*)num = 0;
		*(int*)(num + sizeof(BoundingBoxFace)) = 1;
		*(int*)(num + (nint)2 * (nint)sizeof(BoundingBoxFace)) = 4;
		*(int*)(num + (nint)3 * (nint)sizeof(BoundingBoxFace)) = 5;
		*(int*)(num + (nint)4 * (nint)sizeof(BoundingBoxFace)) = 2;
		*(int*)(num + (nint)5 * (nint)sizeof(BoundingBoxFace)) = 3;
		Span<BoundingBoxFace> span = new Span<BoundingBoxFace>(num, 6);
		ReadOnlySpan<BoundingBoxFace> faces = span;
		ReadOnlySpan<BoundingBoxFace> readOnlySpan = faces;
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			BoundingBoxFace f = readOnlySpan[i];
			float3 handlePos = GetHandlePosition(center_W, dimensions_W, f);
			if (new Bounds(handlePos, HandlePickSize).IntersectRay(ray))
			{
				return new ColliderHandle(handlePos, collider, building, f);
			}
		}
		return null;
	}

	private unsafe void DrawHandles(MapEntity entity, MetaBuildingInternalVariant.CollisionBox collider, CommandBuilder draw)
	{
		if (!Dragging || DraggedColliderHandle == null)
		{
			float3 center_W = entity.W_From_L(in collider.Center_L);
			float3 dimensions_W = collider.DimensionsByRotation_W[(int)entity.Rotation_G];
			Color color = new Color(0f, 1f, 0f, 0.8f);
			byte* num = stackalloc byte[24];
			*(int*)num = 0;
			*(int*)(num + sizeof(BoundingBoxFace)) = 1;
			*(int*)(num + (nint)2 * (nint)sizeof(BoundingBoxFace)) = 4;
			*(int*)(num + (nint)3 * (nint)sizeof(BoundingBoxFace)) = 5;
			*(int*)(num + (nint)4 * (nint)sizeof(BoundingBoxFace)) = 2;
			*(int*)(num + (nint)5 * (nint)sizeof(BoundingBoxFace)) = 3;
			Span<BoundingBoxFace> span = new Span<BoundingBoxFace>(num, 6);
			ReadOnlySpan<BoundingBoxFace> faces = span;
			ReadOnlySpan<BoundingBoxFace> readOnlySpan = faces;
			for (int i = 0; i < readOnlySpan.Length; i++)
			{
				BoundingBoxFace face = readOnlySpan[i];
				draw.SolidBox(GetHandlePosition(center_W, dimensions_W, face), HandleDrawnSize, color);
			}
		}
	}
}
