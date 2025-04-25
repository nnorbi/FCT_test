using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public abstract class GameSettingsGroup : IEquatable<GameSettingsGroup>
{
	private readonly string Id;

	private readonly bool SaveOnChange;

	public List<GameSetting> Settings = new List<GameSetting>();

	public UnityEvent Changed = new UnityEvent();

	private bool DuringLoad = false;

	public string Title => TitleId.tr();

	public string TitleId => "menu.settings." + Id;

	public bool IsModified => Settings.Any((GameSetting s) => s.IsModified);

	protected GameSettingsGroup(string id, bool saveOnChange)
	{
		Id = id;
		SaveOnChange = saveOnChange;
	}

	public bool Equals(GameSettingsGroup other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (Id != other.Id)
		{
			return false;
		}
		if (Settings.Count != other.Settings.Count)
		{
			throw new Exception("Trying to cmopare group with diverging number of settings");
		}
		for (int i = 0; i < Settings.Count; i++)
		{
			if (!Settings[i].Equals(other.Settings[i]))
			{
				return false;
			}
		}
		return true;
	}

	public void CopyFrom(GameSettingsGroup other)
	{
		if (Id != other.Id)
		{
			throw new Exception("Trying to CopyFrom invalid group (" + Id + " vs " + other.Id + ")");
		}
		for (int i = 0; i < other.Settings.Count; i++)
		{
			Settings[i].CopyFrom(other.Settings[i]);
		}
	}

	public void ResetToDefault()
	{
		foreach (GameSetting setting in Settings)
		{
			setting.ResetToDefault();
		}
	}

	protected void Register(GameSetting setting)
	{
		Settings.Add(setting);
		setting.SetParentId(Id);
		setting.Changed.AddListener(OnSettingChanged);
	}

	protected virtual void OnSettingChanged()
	{
		Changed.Invoke();
		if (SaveOnChange)
		{
			Save();
		}
	}

	public virtual void Load()
	{
		if (DuringLoad)
		{
			throw new Exception("Can't call Load() twice.");
		}
		DuringLoad = true;
		try
		{
			foreach (GameSetting setting in Settings)
			{
				setting.Read();
			}
		}
		finally
		{
			DuringLoad = false;
		}
	}

	public void Save()
	{
		if (DuringLoad)
		{
			return;
		}
		Debug.Log("Saving settings: " + Id);
		foreach (GameSetting setting in Settings)
		{
			setting.Write();
		}
		PlayerPrefs.Save();
		Debug.Log("Save player prefs");
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register(Id + ".set", new DebugConsole.StringOption("option"), new DebugConsole.StringOption("value"), delegate(DebugConsole.CommandContext ctx)
		{
			string text = ctx.GetString(0).ToLower();
			GameSetting gameSetting = null;
			foreach (GameSetting current in Settings)
			{
				if (current.Id.ToLower() == text)
				{
					gameSetting = current;
					break;
				}
			}
			if (gameSetting == null)
			{
				ctx.Output("Setting " + text + " not found. Use " + Id + ".list to see the list of settings");
			}
			else if (gameSetting.TrySetFromString(ctx.GetString(1)))
			{
				ctx.Output("Setting value for " + text + " has been updated to " + ctx.GetString(1));
			}
			else
			{
				ctx.Output("Not a valid value: " + ctx.GetString(1));
			}
		});
		console.Register(Id + ".list", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("Available settings (Set with " + Id + ".set <setting> <value>):");
			foreach (GameSetting current in Settings)
			{
				ctx.Output(current.Id + ": " + current.GetHelpText());
			}
		});
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((GameSettingsGroup)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Id, Settings);
	}
}
