using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class BeltReaderEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected const int MIN_CAPTURE_SIZE = 5;

	protected const int MAX_CAPTURE_SIZE = 5;

	protected BeltLane InputLane;

	protected BeltLane OutputLane;

	protected List<ulong> CapturedTimes = new List<ulong>();

	public BeltReaderEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1]);
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], OutputLane);
		OutputLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			CapturedTimes.Add(Island.Simulator.SimulationTick_I - (ulong)remainingTicks_T);
			if (CapturedTimes.Count > 5)
			{
				CapturedTimes.RemoveAt(0);
			}
		};
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, OutputLane })
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected void Draw_Digit(FrameDrawOptions options, int value, ref float3x3 rotationMatrix, float3 offset)
	{
		if (InternalVariant.SupportMeshesInternalLOD[1 + value].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh digitMesh))
		{
			options.RegularRenderer.DrawMesh(digitMesh, FastMatrix.TranslateRotateDegrees(W_From_L(new float3(0f, 0f, 0.55f)) + math.mul(offset, rotationMatrix), options.Player.Viewport.RotationDegrees), options.Theme.BaseResources.BuildingMaterial, RenderCategory.BuildingsDynamic);
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, InputLane);
		DrawDynamic_BeltLane(options, OutputLane);
		float cameraRotation = options.Player.Viewport.RotationDegrees;
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(0), cameraRotation - Grid.DirectionToDegrees(Rotation_G));
		if (CapturedTimes.Count < 5)
		{
			DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[11], new float3(0f, 0f, 0.55f), cameraRotation - Grid.DirectionToDegrees(Rotation_G) + Time.time * 0.2f * 360f);
			return;
		}
		int average_T = 0;
		int lowest_T = int.MaxValue;
		int highest_T = int.MinValue;
		for (int i = 0; i < CapturedTimes.Count - 1; i++)
		{
			int delta = (int)(CapturedTimes[i + 1] - CapturedTimes[i]);
			average_T += delta;
			lowest_T = math.min(lowest_T, delta);
			highest_T = math.max(highest_T, delta);
		}
		average_T /= CapturedTimes.Count - 1;
		float averageSeconds = (float)average_T / (float)IslandSimulator.UPS;
		float itemsPerSecond = 1f / averageSeconds;
		int itemsPerMinute = math.clamp((int)math.round(itemsPerSecond * 60f), 0, 999);
		int digit0 = itemsPerMinute % 10;
		int digit1 = itemsPerMinute / 10 % 10;
		int digit2 = itemsPerMinute / 100;
		float3x3 rotationMatrix = float3x3.RotateY(0f - math.radians(cameraRotation));
		Draw_Digit(options, digit0, ref rotationMatrix, new float3(0.25f, 0f, 0f));
		if (digit1 > 0)
		{
			Draw_Digit(options, digit1, ref rotationMatrix, new float3(0f, 0f, 0f));
		}
		if (digit2 > 0)
		{
			Draw_Digit(options, digit2, ref rotationMatrix, new float3(-0.25f, 0f, 0f));
		}
	}
}
