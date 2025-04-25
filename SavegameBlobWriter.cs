using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

public class SavegameBlobWriter
{
	protected ZipOutputStream Stream;

	protected bool Checkpoints;

	protected SavegameStringLUT StringLUT;

	public Encoding Encoding => SavegameSerializerBase.ENCODING;

	public SavegameBlobWriter(ZipOutputStream stream, bool checkpoints, SavegameStringLUT stringLUT)
	{
		Stream = stream;
		StringLUT = stringLUT;
		Checkpoints = checkpoints;
	}

	public void Write(string filename, Action<BinaryStringLUTSerializationVisitor> handler)
	{
		using MemoryStream memStream = new MemoryStream();
		Stream stream = memStream;
		BinaryStringLUTSerializationVisitor visitor = new BinaryStringLUTSerializationVisitor(writing: true, Checkpoints, Savegame.VERSION, stream, StringLUT);
		handler(visitor);
		WriteRawBlob(filename, memStream.GetBuffer());
	}

	public void WriteObjectAsJson<T>(string filename, T obj)
	{
		string json = JsonConvert.SerializeObject(obj, SavegameSerializerBase.JSON_SETTINGS);
		WriteRawBlob(filename, Encoding.GetBytes(json));
	}

	public void WriteRawBlob(string filename, byte[] contents)
	{
		ZipEntry entry = new ZipEntry(filename);
		entry.Size = contents.Length;
		entry.DateTime = DateTime.Now;
		Stream.PutNextEntry(entry);
		Stream.Write(contents, 0, contents.Length);
		Stream.CloseEntry();
	}
}
