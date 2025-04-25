using Unity.Mathematics;
using UnityEngine;

public class BeltPortReceiverEntityMetaBuildingInternalVariant : MetaBuildingInternalVariant
{
	[Space(20f)]
	public bool RenderDebugCurve;

	public bool OverwriteItemProgress;

	[Range(0f, 1f)]
	public float ItemProgress;

	public AnimationCurve ItemHeightCurve;

	public AnimationCurve ItemRotationCurve;

	public bool RenderItem = true;

	public float3 StopperPosition;

	public float StopperTilt;

	public LOD2Mesh StopperLeft;

	public LOD2Mesh StopperRight;

	public AnimationCurve StopperCurve;

	public bool RenderStoppers = true;
}
