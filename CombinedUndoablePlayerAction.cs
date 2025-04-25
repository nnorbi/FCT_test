using System;
using System.Collections.Generic;

public class CombinedUndoablePlayerAction : IPlayerAction
{
	protected List<IPlayerAction> Actions;

	public Player Executor => Actions[0].Executor;

	public PlayerActionMode Mode => PlayerActionMode.Undoable;

	public CombinedUndoablePlayerAction(List<IPlayerAction> actions)
	{
		Actions = actions;
		foreach (IPlayerAction action in Actions)
		{
			if (action.Executor != Executor)
			{
				throw new Exception("Executor on undoable player action must match, is " + action.Executor?.ToString() + " vs " + Executor);
			}
			if (action.Mode != PlayerActionMode.Undoable)
			{
				throw new Exception("Combined undoable action got sub action that is not undoable");
			}
		}
	}

	public bool IsPossible()
	{
		foreach (IPlayerAction action in Actions)
		{
			if (!action.IsPossible())
			{
				return false;
			}
		}
		return true;
	}

	public IPlayerAction GetReverseActionIfPossible()
	{
		List<IPlayerAction> undoActions = new List<IPlayerAction>();
		foreach (IPlayerAction action in Actions)
		{
			undoActions.Insert(0, action.GetReverseActionIfPossible());
		}
		return new CombinedUndoablePlayerAction(undoActions);
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

	protected void ExecuteInternal()
	{
		foreach (IPlayerAction action in Actions)
		{
			action.TryExecute_INTERNAL(skipChecks_INTERNAL: true);
		}
	}
}
