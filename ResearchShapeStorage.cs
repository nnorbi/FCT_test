using System;
using System.Collections.Generic;
using UnityEngine;

public class ResearchShapeStorage
{
	protected Player Player;

	protected Dictionary<string, Action> OnFirstShapeStored = new Dictionary<string, Action>();

	protected Dictionary<string, ShapesStoredDelegate> OnShapeStored = new Dictionary<string, ShapesStoredDelegate>();

	private ResearchShapeManager ShapeManager;

	protected Dictionary<string, int> StoredShapes => Player.StoredShapes;

	public ResearchShapeStorage(Player player, ResearchShapeManager shapeManager)
	{
		Player = player;
		ShapeManager = shapeManager;
	}

	public int GetAmount(string hash)
	{
		hash = ShapeManager.TrimAndUnifyShape(hash);
		if (StoredShapes.TryGetValue(hash, out var value))
		{
			return value;
		}
		return 0;
	}

	public void Add(ShapeDefinition definition, int amount)
	{
		Add(definition.Hash, amount);
	}

	public void Add(string hash, int amount)
	{
		if (amount < 0)
		{
			throw new Exception("Add(): negative amount: " + amount);
		}
		hash = ShapeManager.TrimAndUnifyShape(hash);
		if (OnShapeStored.TryGetValue(hash, out var handler))
		{
			try
			{
				handler(amount);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
		if (StoredShapes.ContainsKey(hash))
		{
			StoredShapes[hash] += amount;
			return;
		}
		StoredShapes[hash] = amount;
		if (!OnFirstShapeStored.TryGetValue(hash, out var firstTimeHandler))
		{
			return;
		}
		try
		{
			firstTimeHandler();
		}
		catch (Exception exception2)
		{
			Debug.LogException(exception2);
		}
	}

	public void AddFirstShapeStoredHook(string hash, Action handler)
	{
		hash = ShapeManager.TrimAndUnifyShape(hash);
		if (OnFirstShapeStored.ContainsKey(hash))
		{
			Dictionary<string, Action> onFirstShapeStored = OnFirstShapeStored;
			string key = hash;
			onFirstShapeStored[key] = (Action)Delegate.Combine(onFirstShapeStored[key], handler);
		}
		else
		{
			OnFirstShapeStored.Add(hash, handler);
		}
	}

	public void AddShapeStoredHook(string hash, ShapesStoredDelegate callback)
	{
		hash = ShapeManager.TrimAndUnifyShape(hash);
		if (OnShapeStored.ContainsKey(hash))
		{
			Dictionary<string, ShapesStoredDelegate> onShapeStored = OnShapeStored;
			string key = hash;
			onShapeStored[key] = (ShapesStoredDelegate)Delegate.Combine(onShapeStored[key], callback);
		}
		else
		{
			OnShapeStored.Add(hash, callback);
		}
	}

	public void RemoveShapeStoredHook(string hash, ShapesStoredDelegate callback)
	{
		hash = ShapeManager.TrimAndUnifyShape(hash);
		if (OnShapeStored.TryGetValue(hash, out var storedCallback))
		{
			storedCallback = (ShapesStoredDelegate)Delegate.Remove(storedCallback, callback);
			if (storedCallback == null)
			{
				OnShapeStored.Remove(hash);
			}
		}
	}

	public bool CanAfford(string hash, int amount)
	{
		return GetAmount(hash) >= amount;
	}

	public bool TryTake(string hash, int amount)
	{
		hash = ShapeManager.TrimAndUnifyShape(hash);
		if (amount < 0)
		{
			throw new Exception("Negative shape amount: " + amount);
		}
		if (!StoredShapes.ContainsKey(hash))
		{
			return false;
		}
		if (StoredShapes[hash] < amount)
		{
			return false;
		}
		StoredShapes[hash] -= amount;
		return true;
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("hub.list", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("There are " + StoredShapes.Keys.Count + " different shapes stored in the hub: ");
			foreach (KeyValuePair<string, int> current in StoredShapes)
			{
				ctx.Output(" - '" + current.Key + "': " + current.Value);
			}
		});
		console.Register("hub.set", new DebugConsole.StringOption("shape"), new DebugConsole.IntOption("amount", 0), delegate(DebugConsole.CommandContext ctx)
		{
			string text = ctx.GetString(0);
			StoredShapes[text] = ctx.GetInt(1);
			ctx.Output("Amount for " + text + " is now " + GetAmount(text));
		}, isCheat: true);
		console.Register("hub.add", new DebugConsole.StringOption("shape"), new DebugConsole.IntOption("amount", 0), delegate(DebugConsole.CommandContext ctx)
		{
			string text = ctx.GetString(0);
			Add(text, ctx.GetInt(1));
			ctx.Output("Amount for " + text + " is now " + GetAmount(text));
		}, isCheat: true);
		console.Register("hub.clear", delegate(DebugConsole.CommandContext ctx)
		{
			StoredShapes.Clear();
			ctx.Output("Hub storage has been cleared");
		}, isCheat: true);
	}
}
