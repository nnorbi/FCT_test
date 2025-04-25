using UnityEngine;

public class CutterEntityMetaBuildingInternalVariant : MetaBuildingInternalVariant
{
	[Space(20f)]
	public float IndicatorHeightOffset = 0f;

	public AnimationCurve ProgressAdjustmentCurve;

	public LOD2Mesh IndicatorMesh;

	public LOD2Mesh CircleArcMesh;

	public LOD2Mesh StaticSemicircleMesh;

	public float StaticIndicatorHeight;

	public bool RenderStaticSemicircles = true;

	public AnimationCurve ShapeCutCollapseCurve;

	public AnimationCurve ShapeCutDistanceCurve;

	public bool RenderLeftShape = true;

	public bool RenderRightShape = true;

	public AnimationCurve LaserHeightCurve;

	public LOD2Mesh LaserCutterMesh;

	public bool RenderLaserCutter = true;

	public AnimationCurve NewElementRotationCurve;

	public bool RenderNewElements = true;

	public AnimationCurve OldElementRotationCurve;

	public bool RenderDynamicPlatform = true;

	public float RibbonHeightOffset = 0f;

	public float RibbonIdleOffset = 0f;

	public AnimationCurve RibbonHeightBlendCurve;

	public LOD2Mesh RibbonMesh;

	public LOD2Mesh RibbonBaseMesh;

	public bool RenderRibbon = true;
}
