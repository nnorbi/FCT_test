using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

[Serializable]
public class SavegameBlobReader
{
	public Savegame.SerializedMetadata Metadata;

	public string Source;

	public SavegameStringLUT StringLUT;

	public Dictionary<string, byte[]> Blobs;

	public Encoding Encoding => SavegameSerializerBase.ENCODING;

	public SavegameBlobReader(string source = "memory")
	{
		Source = source;
	}

	public void Read(string filename, Action<BinaryStringLUTSerializationVisitor> handler)
	{
		if (StringLUT == null || Metadata == null)
		{
			throw new Exception("Can not read blobs before reading metadata.");
		}
		byte[] bytes = ReadRawBlob(filename);
		using MemoryStream memStream = new MemoryStream(bytes);
		Stream stream = memStream;
		BinaryStringLUTSerializationVisitor visitor = new BinaryStringLUTSerializationVisitor(writing: false, Metadata.BinaryDataCheckpoints, Metadata.Version, stream, StringLUT);
		handler(visitor);
	}

	public T ReadObjectFromJson<T>(string filename)
	{
		byte[] binaryContents = ReadRawBlob(filename);
		string json = SavegameSerializerBase.ENCODING.GetString(binaryContents);
		return JsonConvert.DeserializeObject<T>(json, SavegameSerializerBase.JSON_SETTINGS);
	}

	public byte[] ReadRawBlob(string filename)
	{
		if (Blobs == null)
		{
			throw new Exception("Blobs not read yet.");
		}
		if (!Blobs.ContainsKey(filename))
		{
			throw new Exception("Blob '" + filename + "' not found in savegame.");
		}
		return Blobs[filename];
	}
}
