using System;
using System.IO;
using UnityEngine;

public static class SafeFileWrite
{
	public static void Write(string targetFilename, Action<FileStream> handler)
	{
		string tempPath = Application.temporaryCachePath;
		if (!Directory.Exists(tempPath))
		{
			Directory.CreateDirectory(tempPath);
		}
		string tempFilename = Path.Join(tempPath, DateTimeOffset.Now.ToUnixTimeMilliseconds() + "-" + new System.Random().Next() + ".tmp");
		using (FileStream fileStream = File.Open(tempFilename, FileMode.CreateNew, FileAccess.Write, FileShare.None))
		{
			handler(fileStream);
			if (fileStream.CanWrite)
			{
				fileStream.Flush(flushToDisk: true);
				fileStream.Close();
			}
		}
		string targetFolder = Path.GetDirectoryName(targetFilename);
		if (!Directory.Exists(targetFolder))
		{
			Directory.CreateDirectory(targetFolder);
		}
		if (File.Exists(targetFilename))
		{
			File.Delete(targetFilename);
		}
		File.Move(tempFilename, targetFilename);
	}
}
