using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BeltPathLogic
{
	[Serializable]
	public class ItemOnBelt
	{
		[SerializeReference]
		public BeltItem Item;

		public int NextItemDistance_S;
	}

	public delegate bool TransferHandler(BeltItem item, int excessSteps_S);

	public delegate int GetMinStepsToEndHandler();

	public int FirstItemDistance_S = 0;

	public List<ItemOnBelt> Items = new List<ItemOnBelt>();

	public int Length_S { get; protected set; }

	public BeltItem FirstItem
	{
		get
		{
			if (Items.Count == 0)
			{
				return null;
			}
			return Items[0].Item;
		}
	}

	public BeltItem LastItem
	{
		get
		{
			if (Items.Count == 0)
			{
				return null;
			}
			List<ItemOnBelt> items = Items;
			return items[items.Count - 1].Item;
		}
	}

	public BeltPathLogic(int length_S)
	{
		Length_S = length_S;
		FirstItemDistance_S = Length_S;
	}

	public void Sync(ISerializationVisitor visitor)
	{
		if (visitor.Writing)
		{
			Serialize(visitor);
		}
		else
		{
			Deserialize(visitor);
		}
	}

	public void Serialize(ISerializationVisitor visitor)
	{
		visitor.Checkpoint("belt-path.start");
		visitor.WriteInt_4(FirstItemDistance_S);
		visitor.WriteInt_4(Items.Count);
		for (int i = 0; i < Items.Count; i++)
		{
			ItemOnBelt item = Items[i];
			visitor.WriteString_4(item.Item.Serialize());
			visitor.WriteInt_4(item.NextItemDistance_S);
		}
		visitor.Checkpoint("belt-path.end");
	}

	public void Deserialize(ISerializationVisitor visitor)
	{
		visitor.Checkpoint("belt-path.start");
		FirstItemDistance_S = visitor.ReadInt_4();
		if (Items.Count != 0)
		{
			throw new Exception("Belt path not empty, clear before deserializing. Has " + Items.Count + " items (length_S=" + Length_S + ")");
		}
		int count = visitor.ReadInt_4();
		for (int i = 0; i < count; i++)
		{
			BeltItem item = BeltItem.Deserialize(visitor.ReadString_4());
			int nextDistance = visitor.ReadInt_4();
			Items.Add(new ItemOnBelt
			{
				Item = item,
				NextItemDistance_S = nextDistance
			});
		}
		visitor.Checkpoint("belt-path.end");
	}

	public List<BeltItem> GetItemsAtRange_S(int start_S, int end_S)
	{
		if (start_S < 0 || end_S >= Length_S)
		{
			throw new Exception("Bad path range: " + start_S + " -> " + end_S);
		}
		List<BeltItem> result = new List<BeltItem>();
		int d = FirstItemDistance_S;
		for (int i = 0; i < Items.Count; i++)
		{
			ItemOnBelt item = Items[i];
			if (d >= start_S && d <= end_S)
			{
				result.Add(item.Item);
			}
			d += item.NextItemDistance_S;
		}
		return result;
	}

	public void ClearItems()
	{
		Items.Clear();
		FirstItemDistance_S = Length_S;
	}

	public void ClearItemsBetween(int start_S, int end_S)
	{
		if (Items.Count == 0)
		{
			return;
		}
		int steps_S = FirstItemDistance_S;
		for (int i = 0; i < Items.Count; i++)
		{
			ItemOnBelt item = Items[i];
			if (steps_S >= start_S && steps_S < end_S)
			{
				if (i == 0)
				{
					FirstItemDistance_S += item.NextItemDistance_S;
				}
				else
				{
					ItemOnBelt prevItem = Items[i - 1];
					prevItem.NextItemDistance_S += item.NextItemDistance_S;
				}
				Items.RemoveAt(i);
				i--;
			}
			steps_S += item.NextItemDistance_S;
		}
	}

	public void ExtendPathOnEndBy(int units_S)
	{
		Length_S += units_S;
		if (Items.Count > 0)
		{
			List<ItemOnBelt> items = Items;
			items[items.Count - 1].NextItemDistance_S += units_S;
		}
		else
		{
			FirstItemDistance_S = Length_S;
		}
	}

	public void ExtendPathOnBeginningBy(int units_S)
	{
		Length_S += units_S;
		FirstItemDistance_S += units_S;
	}

	public void ShrinkPathFromBeginningBy(int units_S)
	{
		while (FirstItemDistance_S < units_S && Items.Count > 0)
		{
			ItemOnBelt item = Items[0];
			Items.RemoveAt(0);
			FirstItemDistance_S += item.NextItemDistance_S;
		}
		FirstItemDistance_S -= units_S;
		Length_S -= units_S;
	}

	public void ShrinkPathFromEndBy(int units_S)
	{
		while (Items.Count > 0)
		{
			List<ItemOnBelt> items = Items;
			ItemOnBelt last = items[items.Count - 1];
			int lastDistance = last.NextItemDistance_S;
			if (lastDistance <= units_S)
			{
				Items.RemoveAt(Items.Count - 1);
				if (Items.Count > 0)
				{
					List<ItemOnBelt> items2 = Items;
					items2[items2.Count - 1].NextItemDistance_S += lastDistance;
				}
				else
				{
					FirstItemDistance_S += lastDistance;
				}
				continue;
			}
			break;
		}
		Length_S -= units_S;
		if (Items.Count > 0)
		{
			List<ItemOnBelt> items3 = Items;
			items3[items3.Count - 1].NextItemDistance_S -= units_S;
		}
		else
		{
			FirstItemDistance_S = Length_S;
		}
	}

	public void RemoveLastItem()
	{
		if (Items.Count == 0)
		{
			throw new Exception("Can not remove last item, no item");
		}
		if (Items.Count == 1)
		{
			ClearItems();
			return;
		}
		List<ItemOnBelt> items = Items;
		ItemOnBelt itemOnBelt = items[items.Count - 2];
		int nextItemDistance_S = itemOnBelt.NextItemDistance_S;
		List<ItemOnBelt> items2 = Items;
		itemOnBelt.NextItemDistance_S = nextItemDistance_S + items2[items2.Count - 1].NextItemDistance_S;
		Items.RemoveAt(Items.Count - 1);
	}

	public void SplitPath(int removalStart_S, int removalEnd_S, BeltPathLogic secondPath)
	{
		secondPath.Items = new List<ItemOnBelt>();
		secondPath.FirstItemDistance_S = Length_S - removalEnd_S;
		int oldItemsToKeep = 0;
		int itemPos_S = FirstItemDistance_S;
		for (int i = 0; i < Items.Count; i++)
		{
			ItemOnBelt item = Items[i];
			int advance = item.NextItemDistance_S;
			if (itemPos_S < removalStart_S)
			{
				if (itemPos_S + item.NextItemDistance_S >= removalStart_S)
				{
					item.NextItemDistance_S = removalStart_S - itemPos_S;
				}
				oldItemsToKeep++;
			}
			else if (itemPos_S >= removalEnd_S)
			{
				if (secondPath.Items.Count == 0)
				{
					secondPath.FirstItemDistance_S = itemPos_S - removalEnd_S;
				}
				secondPath.Items.Add(item);
			}
			itemPos_S += advance;
		}
		Items.RemoveRange(oldItemsToKeep, Items.Count - oldItemsToKeep);
		Length_S = removalStart_S;
		if (Items.Count == 0)
		{
			FirstItemDistance_S = Length_S;
		}
	}

	public void AppendOtherPath(BeltPathLogic other)
	{
		if (Items.Count == 0)
		{
			if (other.Items.Count == 0)
			{
				FirstItemDistance_S = Length_S + other.Length_S;
			}
			else
			{
				FirstItemDistance_S = Length_S + other.FirstItemDistance_S;
			}
		}
		else
		{
			List<ItemOnBelt> items = Items;
			items[items.Count - 1].NextItemDistance_S += other.FirstItemDistance_S;
		}
		Items.AddRange(other.Items);
		Length_S += other.Length_S;
	}

	public bool AcceptItem(BeltItem item, int initialDistance_S, int maxProgress_S)
	{
		if (Items.Count == 0)
		{
			int itemPos_S = FastMath.Min(initialDistance_S, maxProgress_S);
			Items.Add(new ItemOnBelt
			{
				Item = item,
				NextItemDistance_S = Length_S - itemPos_S
			});
			FirstItemDistance_S = itemPos_S;
			return true;
		}
		int startDistance_S = FastMath.Min(initialDistance_S, FirstItemDistance_S - 50000);
		if (startDistance_S < 0)
		{
			return false;
		}
		Items.Insert(0, new ItemOnBelt
		{
			Item = item,
			NextItemDistance_S = FirstItemDistance_S - startDistance_S
		});
		FirstItemDistance_S = startDistance_S;
		return true;
	}

	public void Update(TickOptions options, int availableSteps_S, bool endIsConnected, TransferHandler transferHandler, GetMinStepsToEndHandler minStepsHandler_S)
	{
		if (Items.Count == 0)
		{
			return;
		}
		List<ItemOnBelt> items = Items;
		ItemOnBelt lastItem = items[items.Count - 1];
		int minDistanceToEnd_S = minStepsHandler_S();
		while (minDistanceToEnd_S <= 0 && availableSteps_S >= lastItem.NextItemDistance_S)
		{
			int excessSteps_S = availableSteps_S - lastItem.NextItemDistance_S;
			if (transferHandler(lastItem.Item, excessSteps_S))
			{
				Items.RemoveAt(Items.Count - 1);
				if (Items.Count > 0)
				{
					int extraDistance = lastItem.NextItemDistance_S;
					List<ItemOnBelt> items2 = Items;
					lastItem = items2[items2.Count - 1];
					lastItem.NextItemDistance_S += extraDistance;
					minDistanceToEnd_S = minStepsHandler_S();
					continue;
				}
				FirstItemDistance_S = Length_S;
				return;
			}
			break;
		}
		minDistanceToEnd_S = FastMath.Max(1, minDistanceToEnd_S);
		if (lastItem.NextItemDistance_S < minDistanceToEnd_S)
		{
			UnityEngine.Debug.LogWarning("Belt path: Last item CONFLICTS, should have minDistanceToEnd_S= " + minDistanceToEnd_S + " but has NextItemDistance_S = " + lastItem.NextItemDistance_S + " -> Clearing last item");
			RemoveLastItem();
			return;
		}
		int lastItemAdvance_S = FastMath.Min(availableSteps_S, lastItem.NextItemDistance_S - minDistanceToEnd_S);
		lastItem.NextItemDistance_S -= lastItemAdvance_S;
		FirstItemDistance_S += lastItemAdvance_S;
		availableSteps_S -= lastItemAdvance_S;
		int itemCount = Items.Count;
		int newFirstDistance_S = FirstItemDistance_S;
		int i = itemCount - 2;
		while (i >= 0 && availableSteps_S > 0)
		{
			ItemOnBelt item = Items[i];
			int itemMoveSteps_S = FastMath.Min(FastMath.Max(0, item.NextItemDistance_S - 50000), availableSteps_S);
			item.NextItemDistance_S -= itemMoveSteps_S;
			newFirstDistance_S += itemMoveSteps_S;
			availableSteps_S -= itemMoveSteps_S;
			i--;
		}
		FirstItemDistance_S = newFirstDistance_S;
	}

	[Conditional("UNITY_EDITOR")]
	public void Debug_Guard(bool checkLinks = true)
	{
	}
}
