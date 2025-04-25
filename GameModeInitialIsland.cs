using System;

[Serializable]
public struct GameModeInitialIsland
{
	public MetaIslandLayout Layout;

	public Grid.Direction Rotation;

	public GlobalChunkCoordinate Origin_GC;
}
