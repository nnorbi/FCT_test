using UnityEngine;

public class SavegameSerializer : SavegameSerializerBase
{
	protected override void Hook_ApplyAfterSavegameInit(SavegameBlobReader reader, Savegame savegame, GameModeHandle mode, GameContext context)
	{
		mode.Init();
		Debug.Log("SavegameSerializer::Init research after research tree load");
		context.ResearchManager.InitExistingGameFromSerialized(reader);
		Debug.Log("SavegameSerializer::Deserialize local player");
		context.LocalPlayer.Deserialize(reader);
		GameMap mainMap = new GameMap(GameMap.ID_MAIN, new BaseMapInteractionMode(), mode.Config);
		context.Maps.RegisterMap(mainMap);
		context.LocalPlayer.CurrentMap = mainMap;
		Debug.Log("SavegameSerializer::Deserialize map");
		mainMap.Deserialize(reader);
	}

	protected override void Hook_WriteAdditional(SavegameBlobWriter writer, Savegame savegame, GameContext context)
	{
		context.LocalPlayer.Serialize(writer);
		context.ResearchManager.Serialize(writer);
		context.Maps.GetMapById(GameMap.ID_MAIN).Serialize(writer);
	}
}
