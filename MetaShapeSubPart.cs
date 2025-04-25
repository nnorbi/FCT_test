using UnityEngine;

[CreateAssetMenu(fileName = "Shape Part", menuName = "Metadata/Shape Part")]
public class MetaShapeSubPart : ScriptableObject
{
	public char Code;

	[ValidateMesh]
	public Mesh Mesh;

	public bool AllowColor = true;

	public bool AllowChangingColor = true;

	public bool DestroyOnFallDown = false;

	[Header("Override Material")]
	public bool OverrideMaterial = false;

	public ShapeDefinition.ShaderMaterialType Material = ShapeDefinition.ShaderMaterialType.NormalColor;
}
