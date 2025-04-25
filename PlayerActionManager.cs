using System;
using System.Collections.Generic;
using Core.Events;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerActionManager
{
	private static int MAX_STACK_SIZE = 100;

	[NonSerialized]
	private List<IPlayerAction> UndoStack = new List<IPlayerAction>();

	[NonSerialized]
	private List<IPlayerAction> RedoStack = new List<IPlayerAction>();

	private IEventSender PassiveEventBus;

	private Player Player;

	public UnityEvent UndoRedoStackChanged { get; } = new UnityEvent();

	public bool CanUndo => UndoStack.Count > 0;

	public bool CanRedo => RedoStack.Count > 0;

	public PlayerActionManager(Player player, IEventSender passiveEventBus)
	{
		Player = player;
		PassiveEventBus = passiveEventBus;
	}

	public void ClearUndoStack()
	{
		UndoStack.Clear();
		RedoStack.Clear();
		UndoRedoStackChanged.Invoke();
	}

	public void OnGameCleanup()
	{
		UndoStack.Clear();
		RedoStack.Clear();
	}

	public bool ExecuteLocalRaw(IPlayerAction action)
	{
		return action.TryExecute_INTERNAL();
	}

	public bool TryScheduleAction(IPlayerAction action)
	{
		if (!action.IsPossible())
		{
			return false;
		}
		ScheduleAction(action);
		return true;
	}

	public void ScheduleAction(IPlayerAction action)
	{
		if (!action.IsPossible())
		{
			throw new InvalidOperationException("Tried to schedule " + action.GetType().Name + " that is not possible.");
		}
		IPlayerAction reverse = action.GetReverseActionIfPossible();
		if (!ExecuteLocalRaw(action))
		{
			Debug.LogError("Action execute failed (possible=false), but IsPossible() returned true!");
			return;
		}
		switch (action.Mode)
		{
		case PlayerActionMode.Undoable:
			if (action.Executor == Player)
			{
				UndoStack.Add(reverse);
				RedoStack.Clear();
				ClampStacks();
			}
			break;
		case PlayerActionMode.Blocking:
			if (action.Executor == Player)
			{
				ClearUndoStack();
				Debug.Log("Action " + action.GetType().Name + " finished, cleared undo+redo stack");
			}
			break;
		}
		UndoRedoStackChanged.Invoke();
	}

	public bool TryUndo()
	{
		if (!CanUndo)
		{
			return false;
		}
		IPlayerAction last = UndoStack[UndoStack.Count - 1];
		IPlayerAction reverse = last.GetReverseActionIfPossible();
		if (last.TryExecute_INTERNAL())
		{
			RedoStack.Insert(0, reverse);
			UndoStack.RemoveAt(UndoStack.Count - 1);
			ClampStacks();
			UndoRedoStackChanged.Invoke();
			PassiveEventBus.Emit(new PlayerUndoActionEvent(Player));
			return true;
		}
		Debug.LogWarning("Undo of " + last.GetType().Name + " was not possible");
		return false;
	}

	public bool TryRedo()
	{
		if (!CanRedo)
		{
			return false;
		}
		IPlayerAction next = RedoStack[0];
		IPlayerAction reverse = next.GetReverseActionIfPossible();
		if (next.TryExecute_INTERNAL())
		{
			UndoStack.Add(reverse);
			RedoStack.RemoveAt(0);
			ClampStacks();
			UndoRedoStackChanged.Invoke();
			PassiveEventBus.Emit(new PlayerRedoActionEvent(Player));
			return true;
		}
		Debug.LogWarning("Redo of " + next.GetType().Name + " was not possible");
		return false;
	}

	protected void ClampStacks()
	{
		if (UndoStack.Count > MAX_STACK_SIZE)
		{
			UndoStack.RemoveRange(0, UndoStack.Count - MAX_STACK_SIZE);
		}
		if (RedoStack.Count > MAX_STACK_SIZE)
		{
			RedoStack.RemoveRange(MAX_STACK_SIZE, RedoStack.Count - MAX_STACK_SIZE);
		}
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("actions.undo", delegate(DebugConsole.CommandContext ctx)
		{
			if (TryUndo())
			{
				ctx.Output("Undo successful.");
			}
			else
			{
				ctx.Output("No undo possible.");
			}
		});
		console.Register("actions.redo", delegate(DebugConsole.CommandContext ctx)
		{
			if (TryUndo())
			{
				ctx.Output("Undo successful.");
			}
			else
			{
				ctx.Output("No undo possible.");
			}
		});
		console.Register("actions.print-stack", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("Current Action stack (top to bottom):");
			ctx.Output("Can undo: " + CanUndo);
			ctx.Output("Can redo: " + CanRedo);
			ctx.Output("--------------------------------");
			for (int num = RedoStack.Count - 1; num >= 0; num--)
			{
				IPlayerAction playerAction = RedoStack[num];
				ctx.Output(" - [" + num + "] " + playerAction.GetType().Name);
			}
			ctx.Output("<stack pointer>");
			for (int num2 = UndoStack.Count - 1; num2 >= 0; num2--)
			{
				IPlayerAction playerAction2 = UndoStack[num2];
				ctx.Output(" - [" + num2 + "] " + playerAction2.GetType().Name);
			}
			ctx.Output("--------------------------------");
		});
	}
}
