using System;
using System.Collections.Generic;

public class SavegameStringLUT
{
	public const int NULL_INDEX = int.MinValue;

	public static string FILENAME = "strings.bin";

	protected List<string> Strings = new List<string>();

	protected Dictionary<string, int> StringToIndex = new Dictionary<string, int>();

	public int Resolve(string source)
	{
		if (source == null)
		{
			return int.MinValue;
		}
		if (StringToIndex.TryGetValue(source, out var index))
		{
			return index;
		}
		index = Strings.Count;
		Strings.Add(source);
		StringToIndex[source] = index;
		return index;
	}

	public string Resolve(int index)
	{
		if (index == int.MinValue)
		{
			return null;
		}
		if (index < 0 || index >= Strings.Count)
		{
			throw new Exception("Bad string LUT index: " + index + ", have: " + Strings.Count);
		}
		return Strings[index];
	}

	public void Serialize(SavegameBlobWriter writer)
	{
		writer.Write(FILENAME, delegate(BinaryStringLUTSerializationVisitor serializer)
		{
			int count = Strings.Count;
			serializer.WriteInt_4(count);
			for (int i = 0; i < count; i++)
			{
				string s = Strings[i];
				byte[] bytes = writer.Encoding.GetBytes(s);
				serializer.WriteInt_4(bytes.Length);
				serializer.WriteBytesRaw(bytes);
			}
		});
	}

	public void Deserialize(SavegameBlobReader reader)
	{
		reader.Read(FILENAME, delegate(BinaryStringLUTSerializationVisitor serializer)
		{
			Strings.Clear();
			StringToIndex.Clear();
			int num = serializer.ReadInt_4();
			for (int i = 0; i < num; i++)
			{
				int count = serializer.ReadInt_4();
				byte[] bytes = serializer.ReadBytesRaw(count);
				string text = reader.Encoding.GetString(bytes);
				Strings.Add(text);
				StringToIndex[text] = i;
			}
		});
	}
}
