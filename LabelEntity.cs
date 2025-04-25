using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class LabelEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected struct CacheLetterEntry
	{
		public int MeshIndex;

		public float X;
	}

	protected string Text = "decorative-label.default-text-ASCIIONLY".tr();

	protected float CachedScale = 0f;

	protected CacheLetterEntry[] CachedLetters;

	public LabelEntity(CtorArgs payload)
		: base(payload)
	{
		ComputeCache();
	}

	protected override void Hook_SyncConfig(ISerializationVisitor visitor)
	{
		visitor.SyncString_4(ref Text);
		if (!visitor.Writing)
		{
			ComputeCache();
		}
	}

	protected void HUD_ShowConfigureDialog()
	{
		HUDDialogSimpleInput dialog = Singleton<GameCore>.G.HUD.DialogStack.ShowUIDialog<HUDDialogSimpleInput>();
		dialog.InitDialogContents("decorative-label.dialog-title".tr(), "decorative-label.dialog-description".tr(), "global.btn-confirm".tr(), Text);
		dialog.OnConfirmed.AddListener(delegate(string text)
		{
			Text = text.Trim().Substring(0, math.min(text.Length, 36));
			ComputeCache();
			Island.GetChunk_UNSAFE_I(in Tile_I).OnContentChanged();
		});
	}

	protected void ComputeCache()
	{
		string source = Text.ToUpper();
		if (source.Length == 0)
		{
			CachedScale = 0f;
			CachedLetters = null;
			return;
		}
		float spacingBetweenLetters = 0.27f;
		float totalWidth = 0f;
		for (int i = 0; i < source.Length; i++)
		{
			int code = source[i] - 65;
			LODBaseMesh.CachedMesh mesh;
			if (code < 0 || code >= 26)
			{
				totalWidth += 0.38f;
			}
			else if (InternalVariant.SupportMeshesInternalLOD[code].TryGet(0, out mesh))
			{
				float size = mesh.Mesh.bounds.extents.x;
				totalWidth += size;
				if (i != 0)
				{
					totalWidth += spacingBetweenLetters;
				}
			}
		}
		CachedScale = math.min(0.8f, 4.4f / math.max(0.01f, totalWidth));
		float xPos = -2.2f;
		List<CacheLetterEntry> result = new List<CacheLetterEntry>();
		for (int j = 0; j < source.Length; j++)
		{
			int code2 = source[j] - 65;
			LODBaseMesh.CachedMesh mesh2;
			if (code2 < 0 || code2 >= 26)
			{
				xPos += 0.33f * CachedScale;
			}
			else if (InternalVariant.SupportMeshesInternalLOD[code2].TryGet(0, out mesh2))
			{
				float size2 = mesh2.Mesh.bounds.extents.x;
				xPos += size2 / 2f * CachedScale;
				result.Add(new CacheLetterEntry
				{
					MeshIndex = code2,
					X = xPos
				});
				xPos += (size2 / 2f + spacingBetweenLetters) * CachedScale;
			}
		}
		CachedLetters = result.ToArray();
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[1]
		{
			new HUDSidePanelModuleGenericButton("global.btn-configure".tr(), HUD_ShowConfigureDialog)
		};
	}

	protected override void DrawStatic_EndCaps(MeshBuilder builder)
	{
		LOD4Mesh[] standMeshes = Singleton<GameCore>.G.Theme.BaseResources.BeltCapStandsNormal;
		builder.AddTranslateRotate(standMeshes[^1], W_From_L(new float3(-1f, 0f, 0f)), Rotation_G);
		builder.AddTranslateRotate(standMeshes[^1], W_From_L(new float3(2f, 0f, 0f)), Rotation_G);
	}

	public override void DrawStatic_Main(MeshBuilder builder)
	{
		base.DrawStatic_Main(builder);
		if (CachedLetters != null)
		{
			for (int i = 0; i < CachedLetters.Length; i++)
			{
				CacheLetterEntry entry = CachedLetters[i];
				LOD2Mesh mesh = InternalVariant.SupportMeshesInternalLOD[entry.MeshIndex];
				builder.AddTRS(mesh, Matrix4x4.TRS(W_From_L(new float3(entry.X, 0f, 0.28f)), FastMatrix.RotateY(Rotation_G), new float3(CachedScale)));
			}
		}
	}
}
