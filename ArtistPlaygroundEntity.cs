using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class ArtistPlaygroundEntity : MapEntity<MetaBuildingInternalVariant>
{
	public ArtistPlaygroundEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		float degrees = Grid.DirectionToDegrees(Rotation_G);
		using CommandBuilder draw = options.GetDebugDrawManager();
		draw.Label3D(W_From_L(new float3(0f, -4f, 0.5f)), Quaternion.Euler(90f, degrees, 0f), "TOP", 0.4f, LabelAlignment.Center, new Color(0f, 0f, 0f));
		draw.Label3D(W_From_L(new float3(0f, 4f, 0.5f)), Quaternion.Euler(90f, degrees, 0f), "BOTTOM", 0.4f, LabelAlignment.Center, new Color(0f, 0f, 0f));
		draw.Label3D(W_From_L(new float3(-5f, 0f, 0.5f)), Quaternion.Euler(90f, degrees, 0f), "INPUTS", 0.4f, LabelAlignment.Center, new Color(0f, 0f, 0f));
		draw.Label3D(W_From_L(new float3(5f, 0f, 0.5f)), Quaternion.Euler(90f, degrees, 0f), "OUTPUTS", 0.4f, LabelAlignment.Center, new Color(0f, 0f, 0f));
		for (int x = -3; x <= 3; x++)
		{
			for (int y = -3; y <= 3; y++)
			{
				draw.Label3D(W_From_L(new float3(x, y, 0.5f)), Quaternion.Euler(90f, degrees, 0f), "X: " + x + " / Y: " + y, 0.1f, LabelAlignment.Center, new Color(0f, 0f, 0f));
				draw.WireBox(W_From_L(new float3(x, y, 0.3f)), new float3(1f, 0.001f, 1f), new Color(0f, 0f, 0f));
			}
		}
	}
}
