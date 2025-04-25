using System;
using UnityEngine;

[Serializable]
public class MetaIslandChunkBase
{
	public enum EdgeType
	{
		Expand = 1,
		Inset,
		InsetNotch
	}

	[Serializable]
	public struct ExtraMesh
	{
		[ValidateMesh(50)]
		public LOD5Mesh Mesh;

		public Grid.Direction Rotation;
	}

	[Serializable]
	public class SpaceThemeExtraData
	{
		public bool RenderMainFrame = true;

		public bool RenderMainFrameLayer1 = true;

		public bool RenderMainFrameLayer2 = true;

		public bool RenderMainFrameLayer3 = true;

		public bool RenderLowerFrame = true;

		public ExtraMesh[] ExtraMeshes = new ExtraMesh[0];
	}

	public Grid.Direction[] Notches;

	public EditorClassID<IslandChunk> ChunkClass = new EditorClassID<IslandChunk>("IslandChunk");

	public bool RenderPlayingfield = true;

	public bool RenderPlayingfieldCurrentLayerPlane = true;

	public MetaResourcePatchPattern ResourcePatchPattern;

	public EditorClassIDSingleton<IslandChunkPlayingfieldModificator>[] PlayingfieldModificators = new EditorClassIDSingleton<IslandChunkPlayingfieldModificator>[0];

	public IslandChunkCoordinate Tile_IC;

	[Header("Space Theme Extra Data")]
	public SpaceThemeExtraData SpaceTheme;
}
