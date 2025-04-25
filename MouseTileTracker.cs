using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class MouseTileTracker<TCoordinate> where TCoordinate : struct, IEquatable<TCoordinate>
{
	protected Player Player;

	public bool AxialMovementOnly = false;

	protected List<TCoordinate> ChangeStack = new List<TCoordinate>();

	protected GameMap CurrentMap;

	public TCoordinate? CurrentCursorTile { get; protected set; }

	protected MouseTileTracker(Player player, bool axialMovementOnly = false)
	{
		Player = player;
		AxialMovementOnly = axialMovementOnly;
		CurrentMap = Player.CurrentMap;
		CurrentCursorTile = GetCursorTile();
		if (CurrentCursorTile.HasValue)
		{
			ChangeStack.Add(CurrentCursorTile.Value);
		}
	}

	protected abstract TCoordinate? GetCursorTile();

	public TCoordinate[] ConsumeChanges(bool includeCurrent = true)
	{
		if (CurrentCursorTile.HasValue)
		{
			if (includeCurrent)
			{
				if (!IsOnTopOfChangeStack(CurrentCursorTile.Value))
				{
					ChangeStack.Add(CurrentCursorTile.Value);
				}
			}
			else if (IsOnTopOfChangeStack(CurrentCursorTile.Value))
			{
				ChangeStack.RemoveAt(ChangeStack.Count - 1);
			}
		}
		TCoordinate[] result = ChangeStack.ToArray();
		ChangeStack.Clear();
		return result;
		bool IsOnTopOfChangeStack(TCoordinate coordinate)
		{
			int result2;
			if (ChangeStack.Count > 0)
			{
				List<TCoordinate> changeStack = ChangeStack;
				result2 = (changeStack[changeStack.Count - 1].Equals(coordinate) ? 1 : 0);
			}
			else
			{
				result2 = 0;
			}
			return (byte)result2 != 0;
		}
	}

	public void Reset()
	{
		ChangeStack.Clear();
	}

	protected abstract IEnumerable<TCoordinate> FindPathBetween(TCoordinate from, TCoordinate to);

	public void OnGameUpdate()
	{
		TCoordinate? newTile = GetCursorTile();
		if (CurrentMap != Player.CurrentMap)
		{
			Debug.Log("Clearing tile tracker because map changed");
			CurrentMap = Player.CurrentMap;
			CurrentCursorTile = newTile;
			ChangeStack.Clear();
		}
		else if (!CurrentCursorTile.HasValue || !newTile.HasValue)
		{
			CurrentCursorTile = newTile;
		}
		else
		{
			if (CurrentCursorTile.Equals(newTile))
			{
				return;
			}
			foreach (TCoordinate tile in FindPathBetween(CurrentCursorTile.Value, newTile.Value))
			{
				if (ChangeStack.Count != 0)
				{
					List<TCoordinate> changeStack = ChangeStack;
					if (changeStack[changeStack.Count - 1].Equals(tile))
					{
						continue;
					}
				}
				ChangeStack.Add(tile);
			}
			CurrentCursorTile = newTile;
		}
	}
}
