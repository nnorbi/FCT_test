using System;
using System.Runtime.CompilerServices;

public struct IslandDescriptor : IEquatable<IslandDescriptor>
{
	public static readonly IslandDescriptor Invalid;

	public MetaIslandLayout Layout;

	public Grid.Direction LayoutRotation;

	public GlobalChunkCoordinate FirstChunk_GC;

	public static IslandDescriptor From(Island island)
	{
		if (island == null)
		{
			throw new ArgumentNullException("island");
		}
		return new IslandDescriptor(island.Layout.Layout, island.Layout.LayoutRotation, island.Layout.Chunks[0].Tile_IC.To_GC(island));
	}

	public static IslandDescriptor From(MetaIslandLayout layout, Grid.Direction rotation, GlobalChunkCoordinate firstChunk_GC)
	{
		return new IslandDescriptor(layout, rotation, firstChunk_GC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(IslandDescriptor lhs, IslandDescriptor rhs)
	{
		return lhs.Layout == rhs.Layout && lhs.LayoutRotation == rhs.LayoutRotation && lhs.FirstChunk_GC.Equals(rhs.FirstChunk_GC);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(IslandDescriptor lhs, IslandDescriptor rhs)
	{
		return !(lhs == rhs);
	}

	private IslandDescriptor(MetaIslandLayout layout, Grid.Direction rotation, GlobalChunkCoordinate firstChunk_GC)
	{
		if (layout == null)
		{
			throw new ArgumentNullException("layout");
		}
		Layout = layout;
		LayoutRotation = rotation;
		FirstChunk_GC = firstChunk_GC;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(IslandDescriptor other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		return obj is IslandDescriptor other && Equals(other);
	}

	public override int GetHashCode()
	{
		int hash = 17;
		hash = hash * 31 + ((!(Layout == null)) ? Layout.GetHashCode() : 0);
		hash = hash * 31 + LayoutRotation.GetHashCode();
		return hash * 31 + FirstChunk_GC.GetHashCode();
	}

	public override string ToString()
	{
		return string.Format("{0}({1}, {2}, {3})", "IslandDescriptor", FirstChunk_GC, Layout.name, LayoutRotation);
	}
}
