using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Game Mode", menuName = "Metadata/Game Mode")]
public class MetaGameMode : ScriptableObject
{
	[Space(20f)]
	public bool AvailableInDemo = false;

	[Space(20f)]
	[RequiredListLength(1, null)]
	public MetaShapeColor[] ShapeColors;

	[RequiredListLength(1, null)]
	public MetaShapeSubPart[] ShapeSubParts;

	public int MaxShapeLayers;

	[Space(20f)]
	[RequiredListLength(1, null)]
	public MetaBuilding[] Buildings;

	[Space(20f)]
	[RequiredListLength(1, null)]
	public MetaIslandLayout[] IslandLayouts;

	public GameModeInitialIsland[] InitialIslands;

	[Space(20f)]
	public PlayerViewport.SerializedData InitialViewport = new PlayerViewport.SerializedData
	{
		PositionX = -10.4f,
		PositionY = -49.4f,
		Zoom = 13.5f,
		RotationDegrees = -90f,
		Angle = 60f,
		Layer = 0,
		ShowAllLayers = true
	};

	[Space(20f)]
	public MetaResearchTree Research;

	public MetaResearchConfig ResearchConfig;

	[Space(20f)]
	public MapGeneratorData MapGeneratorData;

	[Space(20f)]
	public MetaTutorialConfig Tutorial;

	public string Title => ("menu.game-mode." + base.name + ".title").tr();

	public string Description => ("menu.game-mode." + base.name + ".description").tr();
}
