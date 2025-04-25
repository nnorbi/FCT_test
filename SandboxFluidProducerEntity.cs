using System;
using System.Linq;
using Unity.Mathematics;

[Serializable]
public class SandboxFluidProducerEntity : MapEntity<MetaBuildingInternalVariant>
{
	public Fluid Fluid = null;

	protected FluidContainer Container;

	public SandboxFluidProducerEntity(CtorArgs payload)
		: base(payload)
	{
		Container = new FluidContainer(this, InternalVariant.FluidContainers[0]);
		Fluid = ColorFluid.ForColor(Singleton<GameCore>.G.Mode.ShapeColors[0]);
	}

	public override FluidContainer Fluids_GetContainerByIndex(int n)
	{
		return Container;
	}

	protected override void Hook_SyncConfig(ISerializationVisitor visitor)
	{
		Fluid.Sync(visitor, ref Fluid);
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[2]
		{
			new HUDSidePanelModuleStatFluidProduction(60000f),
			new HUDSidePanelModuleStatFluidCapacity(internalVariant.FluidContainers[0].Max)
		};
	}

	protected void HUD_ShowConfigureDialog()
	{
		HUDDialogSimpleInput dialog = Singleton<GameCore>.G.HUD.DialogStack.ShowUIDialog<HUDDialogSimpleInput>();
		dialog.InitDialogContents("sandbox-fluid-producer.dialog-title".tr(), "sandbox-fluid-producer.dialog-description".tr(), "global.btn-confirm".tr(), ((Fluid as ColorFluid)?.Color.Code ?? 'r').ToString());
		dialog.OnConfirmed.AddListener(delegate(string text)
		{
			text = text.Trim();
			if (string.IsNullOrEmpty(text))
			{
				Fluid = null;
				Container.Flush();
			}
			else
			{
				char code = text[0];
				MetaShapeColor metaShapeColor = Singleton<GameCore>.G.Mode.ShapeColors.FirstOrDefault((MetaShapeColor c) => c.Code == code);
				if (metaShapeColor == null)
				{
					Fluid = null;
					Container.Flush();
				}
				else
				{
					Fluid = ColorFluid.ForColor(metaShapeColor);
					Container.Flush();
					Container.AddAndClamp(10000f, Fluid);
				}
			}
		});
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleGenericButton("global.btn-configure".tr(), HUD_ShowConfigureDialog),
			new HUDSidePanelModuleFluidContainerContents(Container, 10000f)
		};
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		if (Fluid != null)
		{
			Container.AddAndClamp(10000f, Fluid);
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		if (Container.Fluid != null && !(Container.Level < 0.01f) && InternalVariant.SupportMeshesInternalLOD[0].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh mesh))
		{
			options.RegularRenderer.DrawMesh(mesh, FastMatrix.Translate(W_From_L(new float3(0f, 0f, 0f))), Container.Fluid.GetMaterial(), RenderCategory.Fluids);
		}
	}
}
