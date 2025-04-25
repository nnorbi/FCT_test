using System;
using System.Diagnostics;
using UnityEngine;

public static class FolderReveal
{
	public static void Reveal(string folderPath)
	{
		UnityEngine.Debug.Log("Revealing '" + folderPath + "'");
		ProcessStartInfo startInfo = new ProcessStartInfo();
		startInfo.FileName = folderPath;
		try
		{
			Process.Start(startInfo);
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError("Failed to open folder: " + ex);
		}
	}
}
