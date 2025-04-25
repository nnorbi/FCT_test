using System.Collections.Generic;
using UnityEngine;

public class GameSettings
{
	private bool Loaded = false;

	private List<GameSettingsGroup> Groups = new List<GameSettingsGroup>();

	public GraphicSettings Graphics { get; private set; }

	public GeneralGameSettings General { get; private set; }

	public DevSettings Dev { get; private set; }

	public CameraGameSettings Camera { get; private set; }

	public InterfaceGameSettings Interface { get; private set; }

	public DisplaySettings Display { get; private set; }

	public void InitAndLoad()
	{
		if (Loaded)
		{
			Debug.LogError("Can't initialize settings twice");
			return;
		}
		Loaded = true;
		Debug.Log("GameSettings:: Init settings");
		Display = RegisterGroup(new DisplaySettings(saveOnChange: true));
		Graphics = RegisterGroup(new GraphicSettings(saveOnChange: true));
		General = RegisterGroup(new GeneralGameSettings(saveOnChange: true));
		Dev = RegisterGroup(new DevSettings(saveOnChange: true));
		Interface = RegisterGroup(new InterfaceGameSettings(saveOnChange: true));
		Camera = RegisterGroup(new CameraGameSettings(saveOnChange: true));
		Debug.Log("GameSettings:: Load settings");
		Groups.ForEach(delegate(GameSettingsGroup group)
		{
			group.Load();
		});
		Debug.Log("GameSettings:: Settings loaded.");
	}

	private T RegisterGroup<T>(T group) where T : GameSettingsGroup
	{
		Groups.Add(group);
		return group;
	}

	public void RegisterCommands(DebugConsole console)
	{
		Groups.ForEach(delegate(GameSettingsGroup group)
		{
			group.RegisterCommands(console);
		});
	}
}
