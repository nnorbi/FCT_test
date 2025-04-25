using System;
using UnityEngine;

public class ActionSelectBlueprint : PlayerAction
{
	[Serializable]
	public class DataPayload
	{
		public IBlueprint Blueprint;
	}

	protected DataPayload Data;

	public override PlayerActionMode Mode => PlayerActionMode.Undoable;

	public ActionSelectBlueprint(Player executor)
		: base(null, executor)
	{
		Data = new DataPayload
		{
			Blueprint = null
		};
	}

	public ActionSelectBlueprint(Player executor, DataPayload data)
		: base(null, executor)
	{
		Data = data;
	}

	public ActionSelectBlueprint(Player executor, IBlueprint blueprint)
		: base(null, executor)
	{
		Data = new DataPayload
		{
			Blueprint = blueprint
		};
	}

	protected override PlayerAction CreateReverseActionInternal()
	{
		return new ActionSelectBlueprint(base.Executor, base.Executor.CurrentBlueprint.Value);
	}

	public override bool IsPossible()
	{
		if (base.Executor.CurrentBlueprint == Data.Blueprint)
		{
			return false;
		}
		if (Data.Blueprint == null)
		{
			return true;
		}
		if (!Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintsUnlock))
		{
			return false;
		}
		if (Data.Blueprint is IslandBlueprint && !Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.IslandManagementUnlock))
		{
			return false;
		}
		return true;
	}

	protected override void ExecuteInternal()
	{
		base.Executor.CurrentBlueprint.Value = Data.Blueprint;
		if (Data.Blueprint != null && base.Executor == Singleton<GameCore>.G.LocalPlayer && (bool)Globals.Settings.Interface.AutoCopyBpToClipboard)
		{
			GUIUtility.systemCopyBuffer = BlueprintSerializer.Serialize(Data.Blueprint);
		}
	}
}
