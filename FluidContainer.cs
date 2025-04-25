using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FluidContainer
{
	private static int UIDCounter = 1;

	public MetaBuildingInternalVariant.FluidContainerConfig Config;

	public MapEntity Entity;

	[SerializeReference]
	public FluidNetwork Network = null;

	[HideInInspector]
	public List<FluidContainer> ConnectedContainers = new List<FluidContainer>();

	public List<float> FlowRates = new List<float>();

	public int UID;

	public float Value = 0f;

	public float NextValue = 0f;

	public Fluid Fluid = null;

	public string Name => Config.Name;

	public float Max => Config.Max;

	public float Level => Value / Max;

	public float Pressure => Value / Max * 100f;

	public float RemainingSpace => Max - Value;

	public FluidContainer(MapEntity entity, MetaBuildingInternalVariant.FluidContainerConfig config)
	{
		UID = UIDCounter++;
		Config = config;
		Entity = entity;
	}

	public void Sync(ISerializationVisitor visitor)
	{
		if (visitor.Writing)
		{
			if (Fluid == null)
			{
				visitor.WriteByte_1(0);
				return;
			}
			visitor.WriteByte_1(1);
			visitor.WriteFloat_4(Value);
			visitor.WriteString_4(Fluid.Serialize());
		}
		else if (visitor.ReadByte_1() == 1)
		{
			Value = visitor.ReadFloat_4();
			Fluid = Fluid.Deserialize(visitor.ReadString_4());
			NextValue = Value;
		}
	}

	public void Flush()
	{
		Value = 0f;
		Fluid = null;
		NextValue = 0f;
		Entity.Fluids_OnContainedContainerFlushed(this);
	}

	public void Take(float amount)
	{
		Value -= amount;
		NextValue = Value;
		if (Value < 0f)
		{
			throw new Exception("FluidContainer: take: Negative value: " + amount + " -> " + Value + " / " + Max);
		}
	}

	public float ComputeTotalFlow()
	{
		float accumulated = 0f;
		for (int i = 0; i < FlowRates.Count; i++)
		{
			accumulated += GetSignedFlowRateAtIndex(i);
		}
		return accumulated * (float)FluidNetwork.UPS;
	}

	public void Add(float amount, Fluid fluid)
	{
		if (Fluid == null)
		{
			Fluid = fluid;
		}
		else if (Fluid != fluid)
		{
			throw new Exception("FluidContainer: add: mismatching fluid: " + fluid?.ToString() + " vs " + Fluid);
		}
		Value += amount;
		NextValue = Value;
		if (Value > Max)
		{
			throw new Exception("FluidContainer: add: overflow: " + amount + " -> " + Value + " / " + Max);
		}
	}

	public void AddAndClamp(float amount, Fluid fluid)
	{
		if (Fluid == null)
		{
			Fluid = fluid;
		}
		else if (Fluid != fluid)
		{
			throw new Exception("FluidContainer: add: mismatching fluid: " + fluid?.ToString() + " vs " + Fluid);
		}
		Value += amount;
		if (Value > Max)
		{
			Value = Max;
		}
		NextValue = Value;
	}

	public bool HasRightToUpdate(FluidContainer other)
	{
		return UID > other.UID;
	}

	public void Link(FluidContainer other)
	{
		if (ConnectedContainers.Contains(other))
		{
			throw new Exception("double fluid container connection (1)");
		}
		if (other.ConnectedContainers.Contains(this))
		{
			throw new Exception("double fluid container connection (2)");
		}
		ConnectedContainers.Add(other);
		other.ConnectedContainers.Add(this);
		FlowRates.Add(0f);
		other.FlowRates.Add(0f);
	}

	public void Unlink(FluidContainer other)
	{
		int index = ConnectedContainers.IndexOf(other);
		if (index < 0)
		{
			throw new Exception("Missing fluid container connection");
		}
		ConnectedContainers.RemoveAt(index);
		FlowRates.RemoveAt(index);
	}

	public float GetSignedFlowRateAtIndex(int index)
	{
		if (index >= ConnectedContainers.Count)
		{
			throw new Exception("Fluid container: Bad index: " + index);
		}
		FluidContainer other = ConnectedContainers[index];
		if (HasRightToUpdate(other))
		{
			return FlowRates[index];
		}
		int indexInOther = other.ConnectedContainers.IndexOf(this);
		if (indexInOther < 0)
		{
			throw new Exception("Fluid container: Bad link");
		}
		return 0f - other.FlowRates[indexInOther];
	}

	public void UnlinkAll()
	{
		foreach (FluidContainer container in ConnectedContainers)
		{
			container.Unlink(this);
		}
		ConnectedContainers.Clear();
		FlowRates.Clear();
	}
}
