using System;
using Unity.Mathematics;
using UnityEngine;

public class MixerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected struct Chamber
	{
		public Fluid Fluid;

		public float Value;

		public MaterialPropertyBlock PropertyBlock;

		public void Sync(ISerializationVisitor visitor)
		{
			Fluid.Sync(visitor, ref Fluid);
			visitor.SyncFloat_4(ref Value);
		}
	}

	protected enum MixerState : byte
	{
		FillingChambers = 1,
		Mixing,
		Draining
	}

	protected static float CHAMBER_VOLUME = 40f;

	protected static float CHAMBER_FLOW_RATE_PER_SECOND = 20f;

	protected static float MIXING_DURATION = 6f;

	protected FluidContainer Input0Container;

	protected FluidContainer Input1Container;

	protected FluidContainer OutputContainer;

	protected Chamber Chamber0 = new Chamber
	{
		Fluid = null,
		Value = 0f,
		PropertyBlock = new MaterialPropertyBlock()
	};

	protected Chamber Chamber1 = new Chamber
	{
		Fluid = null,
		Value = 0f,
		PropertyBlock = new MaterialPropertyBlock()
	};

	protected MixerState State = MixerState.FillingChambers;

	protected float MixingProgress = 0f;

	protected Fluid MixingResult = null;

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1]
		{
			new HUDSidePanelModuleStatFluidCapacity(internalVariant.FluidContainers[2].Max)
		};
	}

	public MixerEntity(CtorArgs payload)
		: base(payload)
	{
		Input0Container = new FluidContainer(this, InternalVariant.FluidContainers[0]);
		Input1Container = new FluidContainer(this, InternalVariant.FluidContainers[1]);
		OutputContainer = new FluidContainer(this, InternalVariant.FluidContainers[2]);
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		Chamber0.Sync(visitor);
		Chamber1.Sync(visitor);
		visitor.SyncFloat_4(ref MixingProgress);
		Fluid.Sync(visitor, ref MixingResult);
		if (visitor.Writing)
		{
			visitor.WriteByte_1((byte)State);
		}
		else
		{
			State = (MixerState)visitor.ReadByte_1();
		}
	}

	public override FluidContainer Fluids_GetContainerByIndex(int n)
	{
		return n switch
		{
			0 => Input0Container, 
			1 => Input1Container, 
			2 => OutputContainer, 
			_ => throw new ArgumentException("Bad container index: " + n), 
		};
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[3]
		{
			new HUDSidePanelModuleFluidContainerContents(OutputContainer, 20f),
			new HUDSidePanelModuleFluidContainerContents(Input0Container),
			new HUDSidePanelModuleFluidContainerContents(Input1Container)
		};
	}

	protected void DrawContainer(FrameDrawOptions options, FluidContainer container, float3 pos)
	{
		if (container.Fluid != null && InternalVariant.SupportMeshesInternalLOD[0].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh mesh))
		{
			float fluidScale = math.clamp(container.Level / 0.2f, 0.2f, 1f);
			options.RegularRenderer.DrawMesh(mesh, FastMatrix.TranslateScale(W_From_L(in pos), new float3(fluidScale, container.Level, fluidScale)), container.Fluid.GetMaterial(), RenderCategory.Fluids);
		}
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		switch (State)
		{
		case MixerState.FillingChambers:
			FillChamber(options, Input0Container, ref Chamber0, new float3(1f, -0.87f, 0f));
			FillChamber(options, Input1Container, ref Chamber1, new float3(1f, -0.13f, 0f));
			UpdateChamberHoleRadius(ref Chamber0);
			UpdateChamberHoleRadius(ref Chamber1);
			if (Chamber0.Value >= CHAMBER_VOLUME - 0.01f && Chamber1.Value >= CHAMBER_VOLUME - 0.01f)
			{
				MetaShapeColor.ColorMask mixingResult = (Chamber0.Fluid as ColorFluid).Color.Mask | (Chamber1.Fluid as ColorFluid).Color.Mask;
				MetaShapeColor mixedColor = Singleton<GameCore>.G.Mode.GetColorByMask((int)mixingResult);
				if (mixedColor != null)
				{
					MixingResult = ColorFluid.ForColor(mixedColor);
					State = MixerState.Mixing;
					MixingProgress = 0f;
				}
			}
			break;
		case MixerState.Mixing:
		{
			MixingProgress = math.min(1f, MixingProgress + options.DeltaTime / MIXING_DURATION);
			Chamber0.PropertyBlock.SetColor("_F_Color", Chamber0.Fluid.GetMainColor() * (1f - MixingProgress) + MixingResult.GetMainColor() * MixingProgress);
			float holeRadius2 = 0.05f + MixingProgress / 0.3f;
			Chamber1.PropertyBlock.SetColor("_F_Color", Chamber1.Fluid.GetMainColor() * (1f - MixingProgress) + MixingResult.GetMainColor() * MixingProgress);
			if (MixingProgress >= 1f)
			{
				State = MixerState.Draining;
				MixingProgress = 0f;
				float3 holePos = new float3(1.4f, -0.5f, 0f);
				Chamber0.PropertyBlock.SetVector("_HolePosition", (Vector3)W_From_L(in holePos));
				Chamber1.PropertyBlock.SetVector("_HolePosition", (Vector3)W_From_L(in holePos));
			}
			break;
		}
		case MixerState.Draining:
			if (OutputContainer.Fluid == null || OutputContainer.Fluid == MixingResult)
			{
				float drain = math.min(Chamber0.Value * 2f, math.min(OutputContainer.Max - OutputContainer.Value, options.DeltaTime * CHAMBER_FLOW_RATE_PER_SECOND));
				OutputContainer.AddAndClamp(drain, MixingResult);
				Chamber0.Value -= drain / 2f;
				Chamber1.Value -= drain / 2f;
				UpdateChamberHoleRadius(ref Chamber0, 0.25f);
				UpdateChamberHoleRadius(ref Chamber1, 0.25f);
				float holeRadius = math.min(1f, 0.05f + Chamber0.Value / CHAMBER_VOLUME / 0.3f);
				if (Chamber0.Value < 0.01f)
				{
					Chamber0.Value = 0f;
					Chamber0.Fluid = null;
					Chamber1.Value = 0f;
					Chamber1.Fluid = null;
					MixingResult = null;
					MixingProgress = 0f;
					State = MixerState.FillingChambers;
				}
			}
			break;
		}
	}

	protected void UpdateChamberHoleRadius(ref Chamber chamber, float factor = 0.7f)
	{
		float level = chamber.Value / CHAMBER_VOLUME;
		float holeRadius = 0.05f;
		float holeHeight = 0.35f;
		if (level > 0.99f)
		{
			holeRadius = 10f;
		}
		else if (level > holeHeight)
		{
			holeRadius += (level - holeHeight) / factor;
		}
		chamber.PropertyBlock.SetFloat("_HoleRadius", holeRadius);
	}

	protected void FillChamber(TickOptions options, FluidContainer source, ref Chamber chamber, float3 holePos)
	{
		if (source.Fluid != chamber.Fluid)
		{
			if (source.Fluid is ColorFluid)
			{
				chamber.Fluid = source.Fluid;
				chamber.PropertyBlock.SetColor("_F_Color", chamber.Fluid.GetMainColor());
				chamber.PropertyBlock.SetVector("_HolePosition", (Vector3)W_From_L(in holePos));
				chamber.PropertyBlock.SetFloat("_HoleRadius", 0.05f);
			}
			else
			{
				chamber.Fluid = null;
			}
			chamber.Value = 0f;
		}
		if (source.Fluid != null && chamber.Fluid != null)
		{
			float drain = math.min(source.Value, math.min(options.DeltaTime * CHAMBER_FLOW_RATE_PER_SECOND, CHAMBER_VOLUME - chamber.Value));
			chamber.Value += drain;
			source.Take(drain);
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawContainer(options, Input0Container, new float3(0f, -1f, 0.22f));
		DrawContainer(options, Input1Container, new float3(0f, 0f, 0.22f));
		DrawContainer(options, OutputContainer, new float3(2f, 0f, 0.22f));
		float rotation = InternalVariant.GetCurve(0, MixingProgress) * 360f;
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[4], new float3(1f, -0.5f, 0f), rotation);
		if (Chamber0.Fluid != null && InternalVariant.SupportMeshesInternalLOD[3].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh chamberMesh))
		{
			options.RegularRenderer.DrawMesh(chamberMesh, Matrix4x4.TRS(W_From_L(new float3(1f, -0.5f, 0.15f)), FastMatrix.RotateYAngle(Grid.DirectionToDegrees(Rotation_G) + rotation), new float3(1f, Chamber0.Value / CHAMBER_VOLUME, 1f)), Chamber0.Fluid.GetMaterial(), RenderCategory.Fluids);
		}
		if (Chamber1.Fluid != null && InternalVariant.SupportMeshesInternalLOD[2].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh chamberMesh2))
		{
			options.RegularRenderer.DrawMesh(chamberMesh2, Matrix4x4.TRS(W_From_L(new float3(1f, -0.5f, 0.15f)), FastMatrix.RotateYAngle(Grid.DirectionToDegrees(Rotation_G) + rotation), new float3(1f, Chamber1.Value / CHAMBER_VOLUME, 1f)), Chamber1.Fluid.GetMaterial(), RenderCategory.Fluids);
		}
	}
}
