using Unity.Mathematics;
using UnityEngine;

public class StackerEntityMetaBuildingInternalVariant : MetaBuildingInternalVariant
{
	[Space(20f)]
	public bool RenderLaneItems = true;

	public LOD2Mesh MoldLeftMesh;

	public LOD2Mesh MoldRightMesh;

	public AnimationCurve MoldLeftRotationCurve;

	public AnimationCurve MoldRightRotationCurve;

	public AnimationCurve MoldsDistanceCurve;

	public float3 MoldsOffset;

	public bool RenderMolds = true;

	public LOD2Mesh LidLeftMesh;

	public LOD2Mesh LidRightMesh;

	public AnimationCurve LidFactorCurve;

	public bool RenderLids = true;

	public AnimationCurve UpperShapeYOffsetCurve;

	public AnimationCurve WasteAlphaCurve;

	public bool RenderShapes = true;

	public AnimationCurve UpperShapeScaleXCurve;

	public AnimationCurve UpperShapeScaleYCurve;

	public AnimationCurve UpperFloorAlphaCurve;

	public bool RenderShapePlatforms = true;
}
