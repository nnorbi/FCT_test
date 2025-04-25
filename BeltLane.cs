using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class BeltLane
{
	public delegate BeltItem PreAcceptHookDelegate(BeltItem item);

	public delegate void PostAcceptHookDelegate(BeltLane lane, ref int excessTicks_T);

	public delegate int ProgressClampHookDelegate(BeltLane lane, int maxStep_S);

	[SerializeReference]
	public BeltItem Item;

	public BeltLaneDefinition Definition;

	[SerializeReference]
	public BeltLane NextLane;

	public int Progress_T;

	public int MaxStep_S;

	public PreAcceptHookDelegate PreAcceptHook;

	public PostAcceptHookDelegate PostAcceptHook;

	public ProgressClampHookDelegate MaxStepClampHook;

	public bool Empty => Item == null;

	public bool HasItem => Item != null;

	public float Progress => (float)Progress_T / (float)Definition.Duration_T;

	public int MaxTickClamped_T => math.clamp(Definition.T_From_S(MaxStep_S), 0, Definition.Duration_T - 1);

	public BeltLane(BeltLaneDefinition definition, BeltLane next = null)
	{
		Definition = definition;
		Item = null;
		Progress_T = 0;
		MaxStep_S = definition.Length_S;
		NextLane = next;
	}

	public void Sync(ISerializationVisitor visitor)
	{
		if (visitor.Writing)
		{
			if (Item == null)
			{
				visitor.WriteByte_1(0);
				return;
			}
			visitor.WriteByte_1(1);
			visitor.WriteString_4(Item.Serialize());
			visitor.WriteInt_4(Progress_T);
		}
		else if (visitor.ReadByte_1() == 1)
		{
			string itemCode = visitor.ReadString_4();
			Item = BeltItem.Deserialize(itemCode);
			Progress_T = visitor.ReadInt_4();
			if (Progress_T < 0 || Progress_T >= Definition.Duration_T)
			{
				Debug.LogWarning("Bad item on lane " + Definition.Name + ", skipping: Progress_T = " + Progress_T + " but max dur = " + Definition.Duration_T);
				ClearLaneRaw_UNSAFE();
			}
		}
	}

	public void ClearLaneRaw_UNSAFE()
	{
		Item = null;
		Progress_T = 0;
	}

	public void ClearLane()
	{
		if (Item == null)
		{
			Debug.LogError("BeltLane:ClearLane() called but is already empty " + this);
		}
		Item = null;
		Progress_T = 0;
		MaxStep_S = ComputeMaxStepWhenEmptyINTERNAL_S();
	}

	public int ComputeMaxStepWhenEmptyINTERNAL_S()
	{
		int maxStepBase_S = ((NextLane != null) ? (Definition.Length_S + NextLane.MaxStep_S) : (Definition.Length_S - 25000));
		if (MaxStepClampHook != null)
		{
			return MaxStepClampHook(this, maxStepBase_S);
		}
		return maxStepBase_S;
	}

	public override string ToString()
	{
		if (Empty)
		{
			return "{" + Definition.Name + ";i=null;p_max=" + MaxStep_S + "}";
		}
		return "{" + Definition.Name + ";item=" + Item.Serialize() + ";Progress_T=" + Progress_T + ";MaxStep_S=" + MaxStep_S + "}";
	}
}
