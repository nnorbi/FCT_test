using System;
using System.IO;

public class BinarySerializationVisitor : ISerializationVisitor, IBeltLaneTraverser
{
	protected BinaryWriter Writer;

	protected BinaryReader Reader;

	public bool Writing { get; protected set; }

	public bool Checkpoints { get; protected set; }

	public int Version { get; protected set; }

	public BinarySerializationVisitor(bool writing, bool checkpoints, int version, Stream stream)
	{
		Checkpoints = checkpoints;
		Writing = writing;
		Version = version;
		if (Writing)
		{
			Writer = new BinaryWriter(stream, SavegameSerializerBase.ENCODING);
		}
		else
		{
			Reader = new BinaryReader(stream, SavegameSerializerBase.ENCODING);
		}
	}

	public void Checkpoint(string id, bool always = false)
	{
		if (!Checkpoints && !always)
		{
			return;
		}
		byte[] checkpointBytes = SavegameSerializerBase.ENCODING.GetBytes(id);
		int checkpointLength = checkpointBytes.Length;
		if (Writing)
		{
			Writer.Write(checkpointBytes);
			return;
		}
		byte[] buffer = new byte[checkpointLength];
		int count = Reader.Read(buffer, 0, checkpointLength);
		if (count != checkpointLength)
		{
			throw new Exception("Checkpoint mismatch, expected '" + SavegameSerializerBase.ENCODING.GetString(checkpointBytes) + "' but got $EOF$ '" + SavegameSerializerBase.ENCODING.GetString(buffer) + "'");
		}
		for (int i = 0; i < checkpointLength; i++)
		{
			if (buffer[i] != checkpointBytes[i])
			{
				throw new Exception("Checkpoint mismatch, expected '" + SavegameSerializerBase.ENCODING.GetString(checkpointBytes) + "' but got '" + SavegameSerializerBase.ENCODING.GetString(buffer) + "'");
			}
		}
	}

	public void SyncByte_1(ref byte s)
	{
		if (Writing)
		{
			WriteByte_1(s);
		}
		else
		{
			s = ReadByte_1();
		}
	}

	public void SyncBool_1(ref bool s)
	{
		if (Writing)
		{
			WriteBool_1(s);
		}
		else
		{
			s = ReadBool_1();
		}
	}

	public void SyncShort_2(ref short s)
	{
		if (Writing)
		{
			WriteShort_2(s);
		}
		else
		{
			s = ReadShort_2();
		}
	}

	public void SyncInt_4(ref int s)
	{
		if (Writing)
		{
			WriteInt_4(s);
		}
		else
		{
			s = ReadInt_4();
		}
	}

	public void SyncFloat_4(ref float s)
	{
		if (Writing)
		{
			WriteFloat_4(s);
		}
		else
		{
			s = ReadFloat_4();
		}
	}

	public void SyncString_4(ref string s)
	{
		if (Writing)
		{
			WriteString_4(s);
		}
		else
		{
			s = ReadString_4();
		}
	}

	public void Traverse(BeltLane lane)
	{
		lane.Sync(this);
	}

	public void WriteByte_1(byte b)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(b);
	}

	public void WriteBool_1(bool b)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(b);
	}

	public void WriteShort_2(short s)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(s);
	}

	public void WriteInt_4(int s)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(s);
	}

	public void WriteFloat_4(float s)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(s);
	}

	public virtual void WriteString_4(string s)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		if (s == null)
		{
			Writer.Write((short)(-1));
			return;
		}
		if (s.Length == 0)
		{
			Writer.Write((short)0);
			return;
		}
		byte[] bytes = SavegameSerializerBase.ENCODING.GetBytes(s);
		Writer.Write((short)bytes.Length);
		Writer.Write(bytes);
	}

	public void WriteIslandTileCoordinate_6(IslandTileCoordinate coordinate_I)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(coordinate_I.x);
		Writer.Write(coordinate_I.y);
		Writer.Write(coordinate_I.z);
	}

	public void WriteGlobalTileCoordinate_10(GlobalTileCoordinate coordinate_G)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(coordinate_G.x);
		Writer.Write(coordinate_G.y);
		Writer.Write(coordinate_G.z);
	}

	public void WriteGlobalChunkCoordinate_8(GlobalChunkCoordinate coordinate_GC)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(coordinate_GC.x);
		Writer.Write(coordinate_GC.y);
	}

	public void WriteBytesRaw(byte[] s)
	{
		if (!Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(s);
	}

	public byte ReadByte_1()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		return Reader.ReadByte();
	}

	public bool ReadBool_1()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		return Reader.ReadBoolean();
	}

	public short ReadShort_2()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		return Reader.ReadInt16();
	}

	public int ReadInt_4()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		return Reader.ReadInt32();
	}

	public float ReadFloat_4()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		return Reader.ReadSingle();
	}

	public virtual string ReadString_4()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		short length = Reader.ReadInt16();
		if (length < 0)
		{
			if (length != -1)
			{
				throw new Exception("Bad string length: " + length);
			}
			return null;
		}
		if (length == 0)
		{
			return "";
		}
		byte[] bytes = Reader.ReadBytes(length);
		return SavegameSerializerBase.ENCODING.GetString(bytes);
	}

	public IslandTileCoordinate ReadIslandTileCoordinate_6()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		short x = Reader.ReadInt16();
		short y = Reader.ReadInt16();
		short z = Reader.ReadInt16();
		return new IslandTileCoordinate(x, y, z);
	}

	public GlobalTileCoordinate ReadGlobalTileCoordinate_10()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		int x = Reader.ReadInt32();
		int y = Reader.ReadInt32();
		short z = Reader.ReadInt16();
		return new GlobalTileCoordinate(x, y, z);
	}

	public GlobalChunkCoordinate ReadGlobalChunkCoordinate_8()
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		int x = Reader.ReadInt32();
		int y = Reader.ReadInt32();
		return new GlobalChunkCoordinate(x, y);
	}

	public byte[] ReadBytesRaw(int count)
	{
		if (Writing)
		{
			throw new Exception("Not in read mode");
		}
		return Reader.ReadBytes(count);
	}
}
