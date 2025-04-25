using System;
using System.IO;

public class BinaryStringLUTSerializationVisitor : BinarySerializationVisitor
{
	protected SavegameStringLUT StringLUT;

	public BinaryStringLUTSerializationVisitor(bool writing, bool checkpoints, int version, Stream stream, SavegameStringLUT stringLUT)
		: base(writing, checkpoints, version, stream)
	{
		StringLUT = stringLUT;
	}

	public override void WriteString_4(string s)
	{
		if (!base.Writing)
		{
			throw new Exception("Not in write mode");
		}
		Writer.Write(StringLUT.Resolve(s));
	}

	public override string ReadString_4()
	{
		if (base.Writing)
		{
			throw new Exception("Not in read mode");
		}
		return StringLUT.Resolve(Reader.ReadInt32());
	}
}
