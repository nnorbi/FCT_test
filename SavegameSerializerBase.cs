using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using UnityEngine;

public class SavegameSerializerBase
{
	[Serializable]
	public class GameContext
	{
		public Player LocalPlayer;

		public MapManager Maps;

		public ResearchManager ResearchManager;
	}

	public static Encoding ENCODING = Encoding.UTF8;

	public static JsonSerializerSettings JSON_SETTINGS = new JsonSerializerSettings
	{
		Culture = CultureInfo.InvariantCulture,
		MissingMemberHandling = MissingMemberHandling.Ignore,
		ObjectCreationHandling = ObjectCreationHandling.Replace,
		StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
		DefaultValueHandling = DefaultValueHandling.Ignore
	};

	public static bool WRITE_HEADER = false;

	public static byte[] BINARY_HEADER = ENCODING.GetBytes("SHAPEZ2_V1_ZIP");

	protected static bool ONGOING_OPERATION = false;

	protected T LockAndExecute<T>(Func<T> handler)
	{
		if (ONGOING_OPERATION)
		{
			Debug.LogWarning("Duplicate ongoing operation");
			return default(T);
		}
		ONGOING_OPERATION = true;
		try
		{
			return handler();
		}
		finally
		{
			ONGOING_OPERATION = false;
		}
	}

	public SavegameBlobReader Read(string filename, bool metadataOnly = false)
	{
		return LockAndExecute(delegate
		{
			using FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None);
			return ReadFromStreamInternal(inputStream, metadataOnly, filename);
		});
	}

	public SavegameBlobReader ReadFromStream(MemoryStream stream, bool metadataOnly = false)
	{
		return LockAndExecute(() => ReadFromStreamInternal(stream, metadataOnly, "memory"));
	}

	public void InitializeSavegame(SavegameBlobReader reader, GameContext context, out Savegame savegame, out GameModeHandle mode)
	{
		if (reader.Metadata.Version < Savegame.LOWEST_SUPPORTED_VERSION)
		{
			throw new Exception("Savegame has version " + reader.Metadata.Version + " but lowest supported version is " + Savegame.LOWEST_SUPPORTED_VERSION);
		}
		if (reader.Metadata.Version > Savegame.VERSION)
		{
			throw new Exception("Savegame has version " + reader.Metadata.Version + " but highest supported version is " + Savegame.VERSION);
		}
		savegame = Savegame.CreateFromReader(reader);
		mode = new GameModeHandle(savegame.ModeConfig);
		Hook_ApplyAfterSavegameInit(reader, savegame, mode, context);
	}

	public void Write(Savegame savegame, GameContext context, string filename)
	{
		LockAndExecute(delegate
		{
			SafeFileWrite.Write(filename, delegate(FileStream stream)
			{
				WriteToStream(stream, savegame, context);
			});
			return true;
		});
	}

	protected SavegameBlobReader ReadFromStreamInternal(Stream inputStream, bool metadataOnly, string source = "stream")
	{
		ReadHeader(inputStream);
		SavegameBlobReader reader = new SavegameBlobReader(source);
		reader.StringLUT = new SavegameStringLUT();
		reader.Blobs = ReadBlobsFromZipStream(inputStream, metadataOnly);
		reader.Metadata = Savegame.DeserializeMetadata(reader);
		if (reader.Metadata.Version != Savegame.VERSION)
		{
			Debug.Log("Savegame version mismatch: Savegame has " + reader.Metadata.Version + ", current is " + Savegame.VERSION + " minimum is " + Savegame.LOWEST_SUPPORTED_VERSION);
		}
		if (!metadataOnly)
		{
			reader.StringLUT.Deserialize(reader);
		}
		Debug.Log("SavegameSerializer:: Read savegame with game mode " + reader.Metadata.GameMode.GameModeId + ", checkpoints: " + reader.Metadata.BinaryDataCheckpoints);
		return reader;
	}

	protected void ReadHeader(Stream inputStream)
	{
		if (WRITE_HEADER)
		{
			byte[] header = BINARY_HEADER;
			if (inputStream.Length < header.Length)
			{
				throw new Exception("Bad header, file too short");
			}
			byte[] result = new byte[header.Length];
			inputStream.Read(result, 0, header.Length);
			if (!header.SequenceEqual(result))
			{
				throw new Exception("Bad header, File might be corrupted");
			}
		}
	}

	protected Dictionary<string, byte[]> ReadBlobsFromZipStream(Stream inputStream, bool metadataOnly)
	{
		Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();
		using (ZipFile zipFile = new ZipFile(inputStream, leaveOpen: false, StringCodec.FromEncoding(ENCODING)))
		{
			byte[] buffer = new byte[4096];
			foreach (ZipEntry zipEntry in zipFile)
			{
				if (zipEntry.IsDirectory || (metadataOnly && zipEntry.Name != Savegame.META_FILENAME))
				{
					continue;
				}
				using Stream entryStream = zipFile.GetInputStream(zipEntry);
				using MemoryStream memStream = new MemoryStream();
				StreamUtils.Copy(entryStream, memStream, buffer);
				files[zipEntry.Name] = memStream.GetBuffer();
			}
		}
		return files;
	}

	protected virtual void Hook_ApplyAfterSavegameInit(SavegameBlobReader reader, Savegame savegame, GameModeHandle mode, GameContext context)
	{
	}

	protected void WriteToStream(Stream outputStream, Savegame savegame, GameContext context)
	{
		if (WRITE_HEADER)
		{
			outputStream.Write(BINARY_HEADER);
		}
		using ZipOutputStream zipStream = new ZipOutputStream(outputStream, StringCodec.FromEncoding(ENCODING));
		SavegameStringLUT stringLUT = new SavegameStringLUT();
		SavegameBlobWriter writer = new SavegameBlobWriter(zipStream, savegame.Meta.BinaryDataCheckpoints, stringLUT);
		savegame.SerializeMetadata(writer, context);
		Hook_WriteAdditional(writer, savegame, context);
		stringLUT.Serialize(writer);
		zipStream.Finish();
		zipStream.Close();
	}

	protected virtual void Hook_WriteAdditional(SavegameBlobWriter writer, Savegame savegame, GameContext context)
	{
	}
}
