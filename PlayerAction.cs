using System;
using UnityEngine;

[Serializable]
public abstract class PlayerAction : IPlayerAction
{
	public GameMap Map;

	public Player Executor { get; }

	public abstract PlayerActionMode Mode { get; }

	protected PlayerAction(GameMap map, Player executor)
	{
		Executor = executor;
		Map = map;
	}

	public abstract bool IsPossible();

	public IPlayerAction GetReverseActionIfPossible()
	{
		if (Mode == PlayerActionMode.Undoable)
		{
			return CreateReverseActionInternal();
		}
		return null;
	}

	public bool TryExecute_INTERNAL(bool skipChecks_INTERNAL = false)
	{
		if (!skipChecks_INTERNAL && !IsPossible())
		{
			return false;
		}
		ExecuteInternal();
		return true;
	}

	protected virtual PlayerAction CreateReverseActionInternal()
	{
		throw new NotImplementedException();
	}

	protected abstract void ExecuteInternal();

	protected bool ExecuteChildAction(PlayerAction action)
	{
		Debug.Log("Executing child action " + action.GetType().Name + " from " + GetType().Name);
		if (!action.IsPossible())
		{
			Debug.LogError("child action " + action.GetType().Name + " from " + GetType().Name + " is not possible!");
			return false;
		}
		action.ExecuteInternal();
		return true;
	}
}
