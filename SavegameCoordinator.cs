using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SavegameCoordinator
{
	private GameStartOptions Options;

	private SavegameSerializer Serializer;

	private ISavegameManager SavegameManager;

	public bool Headless => Options.MenuMode;

	public bool IsFreshGame => Options is GameStartOptionsStartNew;

	public SavegameCoordinator(GameStartOptions options, ISavegameManager savegameManager)
	{
		Serializer = new SavegameSerializer();
		Options = options;
		SavegameManager = savegameManager;
	}

	protected void EditorCreateOptionsIfNull()
	{
		if (Options != null)
		{
			return;
		}
		SavegameReference editorGame = SavegameManager.FindMostRecentSavegameEntryByUID("editor");
		if (editorGame == null)
		{
			MetaGameMode fallback = Globals.Resources.SupportedGameModes.First((MetaGameMode mode) => !mode.AvailableInDemo);
			if ((bool)Globals.Settings.Dev.StartNewGamesInDemoEditorOnly || GameEnvironmentManager.IS_DEMO)
			{
				fallback = Globals.Resources.SupportedGameModes.First((MetaGameMode mode) => mode.AvailableInDemo);
			}
			Debug.Log("SavegameCoordinator:: No options and no editor savegame, choosing fallback game mode " + fallback.name);
			Options = new GameStartOptionsStartNew
			{
				MenuMode = false,
				UID = "editor",
				Config = new GameModeConfig(fallback, RandomUtils.NextInt(10000))
			};
		}
		else
		{
			Debug.Log("SavegameCoordinator:: Continuing editor savegame at snapshot " + editorGame.SnapshotIndex);
			SavegameBlobReader reader = Serializer.Read(editorGame.FullPath);
			Options = new GameStartOptionsContinueExisting
			{
				MenuMode = false,
				SavegameReader = reader,
				UID = editorGame.UID
			};
		}
	}

	public void InitAfterCoreLoad()
	{
		if (Options is GameStartOptionsContinueExisting continueOptions)
		{
			InitExistingGame(continueOptions);
		}
		else
		{
			if (!(Options is GameStartOptionsStartNew startNewOptions))
			{
				throw new Exception("Unknown game start option: " + Options);
			}
			InitNewGame(startNewOptions);
		}
		foreach (GameMap map in Singleton<GameCore>.G.Maps.GetAllMaps())
		{
			map.PopulateCaches();
		}
	}

	protected void InitExistingGame(GameStartOptionsContinueExisting options)
	{
		Debug.Log("Core::Continue savegame with mode " + options.SavegameReader.Metadata.GameMode.GameModeId);
		try
		{
			Serializer.InitializeSavegame(options.SavegameReader, new SavegameSerializerBase.GameContext
			{
				Maps = Singleton<GameCore>.G.Maps,
				LocalPlayer = Singleton<GameCore>.G.LocalPlayer,
				ResearchManager = Singleton<GameCore>.G.Research
			}, out Singleton<GameCore>.G.Savegame, out Singleton<GameCore>.G.Mode);
			if (!Singleton<GameCore>.G.Maps.HasMap(GameMap.ID_MAIN))
			{
				throw new Exception("No main map in savegame");
			}
		}
		catch (Exception ex)
		{
			if (!Options.MenuMode)
			{
				Debug.LogError("Failed to load '" + options.SavegameReader.Source + "': " + ex);
				throw;
			}
			Debug.LogWarning("Failed to load '" + options.SavegameReader.Source + "': " + ex);
			InitNewGame(new GameStartOptionsStartNew
			{
				Config = new GameModeConfig(Globals.Resources.SupportedGameModes.First(), 0),
				MenuMode = true,
				UID = "menu"
			});
		}
		options.SavegameReader = null;
	}

	protected void InitNewGame(GameStartOptionsStartNew options)
	{
		Debug.Log("Core::Start new savegame with mode " + options.Config);
		Singleton<GameCore>.G.Savegame = Savegame.CreateNew(options.Config);
		Singleton<GameCore>.G.Mode = new GameModeHandle(options.Config);
		Singleton<GameCore>.G.Mode.Init();
		Singleton<GameCore>.G.Research.InitNewGame();
		GameMap mainMap = new GameMap(GameMap.ID_MAIN, new BaseMapInteractionMode(), options.Config);
		Singleton<GameCore>.G.LocalPlayer.CurrentMap = mainMap;
		Singleton<GameCore>.G.Maps.RegisterMap(mainMap);
		mainMap.PlaceInitialIslands();
		Singleton<GameCore>.G.LocalPlayer.Viewport.Deserialize(Singleton<GameCore>.G.Mode.InitialViewport);
	}

	public void SaveCurrentSync()
	{
		if (!Options.MenuMode)
		{
			Debug.Log("SavegameCoordinator:: Saving game");
			try
			{
				string filename = SavegameManager.BuildSavegameNameFromUID(Options.UID);
				Singleton<GameCore>.G.Savegame.Meta.Version = Savegame.VERSION;
				Serializer.Write(Singleton<GameCore>.G.Savegame, new SavegameSerializerBase.GameContext
				{
					Maps = Singleton<GameCore>.G.Maps,
					LocalPlayer = Singleton<GameCore>.G.LocalPlayer,
					ResearchManager = Singleton<GameCore>.G.Research
				}, filename);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to save: " + ex);
				Singleton<GameCore>.G.CrashWithFatalError("Failed to save '" + Options.UID + "': " + ex);
			}
			SavegameManager.CleanupOldSavegameBackups(Options.UID);
		}
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("savegames.info", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("Start Options: " + Options.GetType().Name);
			ctx.Output("Game Mode: " + Singleton<GameCore>.G.Mode.BaseId);
			ctx.Output("Headless: " + Headless);
			ctx.Output("Savegame UID: " + Options.UID);
			ctx.Output("Savegame Version: " + Singleton<GameCore>.G.Savegame.Meta.Version);
			ctx.Output("Savegame Source Version: " + Singleton<GameCore>.G.Savegame.Meta.AppSourceVersion);
			ctx.Output("Savegame Source Store: " + Singleton<GameCore>.G.Savegame.Meta.AppSourceStore);
			ctx.Output("Savegame Source Environment: " + Singleton<GameCore>.G.Savegame.Meta.AppSourceEnvironment);
			ctx.Output("Savegame Binary Checkpoints: " + Singleton<GameCore>.G.Savegame.Meta.BinaryDataCheckpoints);
			ctx.Output("Savegame Last Saved: " + Singleton<GameCore>.G.Savegame.Meta.LastSaved);
			ctx.Output("Savegame Total Playtime: " + Singleton<GameCore>.G.Savegame.Meta.TotalPlaytime);
			ctx.Output("Savegame Research Progress: " + Singleton<GameCore>.G.Savegame.Meta.ResearchProgress);
			ctx.Output("Savegame Structure Count: " + Singleton<GameCore>.G.Savegame.Meta.StructureCount);
		});
		console.Register("savegames.list", delegate(DebugConsole.CommandContext ctx)
		{
			List<SavegameReference> list = SavegameManager.DiscoverAllSavegames();
			ctx.Output("Found " + list.Count + " savegames:");
			foreach (SavegameReference current in list)
			{
				ctx.Output(" Savegame [" + current.UID + "] Snapshot: " + current.SnapshotIndex + " Last changed: " + current.LastChange);
				ctx.Output(" -> File: " + current.FullPath);
			}
		});
		console.Register("savegames.save", delegate(DebugConsole.CommandContext ctx)
		{
			SaveCurrentSync();
			ctx.Output("Saved game with uid " + Options.UID);
		});
		console.Register("savegames.load", new DebugConsole.StringOption("uid"), delegate(DebugConsole.CommandContext ctx)
		{
			string value = ctx.GetString(0);
			List<SavegameReference> list = SavegameManager.DiscoverAllSavegames();
			foreach (SavegameReference current in list)
			{
				if (current.UID.StartsWith(value))
				{
					ctx.Output("Loading savegame with uid " + current.UID + " snapshot " + current.SnapshotIndex + " from " + current.FullPath);
					SavegameBlobReader savegameReader = Serializer.Read(current.FullPath);
					Options = new GameStartOptionsContinueExisting
					{
						MenuMode = Options.MenuMode,
						SavegameReader = savegameReader,
						UID = current.UID
					};
					InitAfterCoreLoad();
					ctx.Output("Successfully loaded game " + current.UID);
					return;
				}
			}
			ctx.Output("No savegame with the given UID filter found.");
		});
		console.Register("savegames.set-binary-checkpoints", new DebugConsole.BoolOption("checkpoints"), delegate(DebugConsole.CommandContext ctx)
		{
			Singleton<GameCore>.G.Savegame.Meta.BinaryDataCheckpoints = ctx.GetBool(0);
		});
		console.Register("savegames.open-folder", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("Opening " + GameEnvironmentManager.DATA_PATH);
			FolderReveal.Reveal(GameEnvironmentManager.DATA_PATH);
		});
	}
}
