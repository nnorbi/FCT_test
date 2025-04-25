using UnityEngine;

[CreateAssetMenu(fileName = "SpaceThemeIslandResources", menuName = "Metadata/Themes/Space/Island Resources")]
public class SpaceThemeIslandResources : ScriptableObject
{
	[Header("Play Area Border")]
	[Space(10f)]
	public LOD5Mesh[] PlayingfieldBorderCornerConcave;

	public LOD5Mesh[] PlayingfieldBorderCornerConvex;

	public LOD5Mesh[] PlayingfieldBorderWall4m;

	public LOD5Mesh[] PlayingfieldBorderWall6m;

	public LOD5Mesh[] PlayingfieldBorderNotch6m;

	[Header("Play Area Greeble")]
	[Space(10f)]
	public LOD2Mesh[] PlayingfieldGreebleCornerConcave;

	public LOD2Mesh[] PlayingfieldGreebleCornerConvex;

	public LOD2Mesh[] PlayingfieldGreebleWall4m;

	public LOD2Mesh[] PlayingfieldGreebleWall6m;

	public LOD2Mesh[] PlayingfieldGreebleNotch6m;

	[Header("Layer 1")]
	[Space(10f)]
	public LOD5Mesh[] Layer1CornerConcave;

	public LOD5Mesh[] Layer1CornerConvex;

	public LOD5Mesh[] Layer1Wall4m;

	public LOD5Mesh[] Layer1Wall6m;

	public LOD5Mesh[] Layer1Notch6m;

	[Header("Layer 2")]
	[Space(10f)]
	public LOD5Mesh[] Layer2CornerConcave;

	public LOD5Mesh[] Layer2CornerConvex;

	public LOD5Mesh[] Layer2Wall4m;

	public LOD5Mesh[] Layer2Wall6m;

	[Header("Layer 3")]
	[Space(10f)]
	public LOD5Mesh[] Layer3CornerConcave;

	public LOD5Mesh[] Layer3CornerConvex;

	public LOD5Mesh[] Layer3Wall6m;

	public LOD5Mesh[] Layer3ConnectedWall6m;

	public LOD5Mesh[] Layer3Wall7m;

	[Header("Lower Frames - Normal")]
	[Space(10f)]
	public LOD5Mesh[] LowerFrameBase;

	public LOD5Mesh[] LowerFrameInsideConnector;

	public LOD5Mesh[] LowerFrameCornerDecoration;

	public LOD5Mesh[] LowerFrameOutsideConnector;

	[Range(0f, 1f)]
	[Space(10f)]
	public float LowerFrameCornerDecorationLikeliness = 0.336f;

	[Header("Lower Frames - Small")]
	[Space(10f)]
	public LOD5Mesh[] LowerFrameBaseSmall;

	public LOD5Mesh[] LowerFrameInsideConnectorSmall;

	public LOD5Mesh[] LowerFrameOutsideConnectorSmall;

	[Range(0f, 1f)]
	[Space(10f)]
	public float LowerFrameSmallCornerDecorationLikeliness = 0.374f;
}
