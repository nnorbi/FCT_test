public interface ISerializationVisitor : IBeltLaneTraverser
{
	bool Writing { get; }

	bool Checkpoints { get; }

	int Version { get; }

	void Checkpoint(string id, bool always = false);

	void SyncByte_1(ref byte s);

	void SyncBool_1(ref bool s);

	void SyncShort_2(ref short s);

	void SyncInt_4(ref int s);

	void SyncFloat_4(ref float s);

	void SyncString_4(ref string s);

	void WriteByte_1(byte b);

	void WriteBool_1(bool b);

	void WriteShort_2(short s);

	void WriteInt_4(int s);

	void WriteFloat_4(float s);

	void WriteString_4(string s);

	void WriteIslandTileCoordinate_6(IslandTileCoordinate coordinate_I);

	void WriteGlobalTileCoordinate_10(GlobalTileCoordinate coordinate_G);

	void WriteGlobalChunkCoordinate_8(GlobalChunkCoordinate coordinate_GC);

	void WriteBytesRaw(byte[] s);

	byte ReadByte_1();

	bool ReadBool_1();

	short ReadShort_2();

	int ReadInt_4();

	float ReadFloat_4();

	string ReadString_4();

	IslandTileCoordinate ReadIslandTileCoordinate_6();

	GlobalTileCoordinate ReadGlobalTileCoordinate_10();

	GlobalChunkCoordinate ReadGlobalChunkCoordinate_8();
}
