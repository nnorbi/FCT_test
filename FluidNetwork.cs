using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class FluidNetwork
{
	public static int UPS = 40;

	public static float FIXED_UPDATE_DURATION_SECONDS = 1f / (float)UPS;

	public static int MAX_QUEUED_UPDATES = 30;

	public static float MAX_ACCUMULATED_SECONDS = (float)MAX_QUEUED_UPDATES * FIXED_UPDATE_DURATION_SECONDS;

	public static int NEXT_UID = 0;

	public static int STATS_FIXED_TICKS = 0;

	public int UID = 0;

	public List<FluidContainer> ContainersList = new List<FluidContainer>();

	protected float AccumulatedDeltaTime = 0f;

	[HideInInspector]
	public HashSet<FluidContainer> Containers = new HashSet<FluidContainer>();

	public FluidNetwork()
	{
		UID = ++NEXT_UID;
	}

	public void FlushAll()
	{
		for (int i = 0; i < Containers.Count; i++)
		{
			FluidContainer container = ContainersList[i];
			container.Flush();
		}
	}

	protected float ClampFlow(FluidContainer from, FluidContainer to, float flow)
	{
		flow = math.min(to.RemainingSpace * 0.25f, flow);
		flow = math.min(from.Value * 0.25f, flow);
		return flow;
	}

	public void Update(TickOptions options)
	{
		AccumulatedDeltaTime += options.DeltaTime;
		if (AccumulatedDeltaTime > MAX_ACCUMULATED_SECONDS)
		{
			Debug.LogWarning("Fluid network: AccumulatedDeltaTime > max, clamping from " + AccumulatedDeltaTime + " to " + MAX_ACCUMULATED_SECONDS);
			AccumulatedDeltaTime = MAX_ACCUMULATED_SECONDS;
		}
		while (AccumulatedDeltaTime > FIXED_UPDATE_DURATION_SECONDS)
		{
			AccumulatedDeltaTime -= FIXED_UPDATE_DURATION_SECONDS;
			FixedUpdate();
		}
	}

	protected void FixedUpdate()
	{
		int containerCount = ContainersList.Count;
		STATS_FIXED_TICKS++;
		for (int i = 0; i < containerCount; i++)
		{
			FluidContainer container = ContainersList[i];
			List<FluidContainer> connections = container.ConnectedContainers;
			int connectionCount = connections.Count;
			for (int connectionIndex = 0; connectionIndex < connectionCount; connectionIndex++)
			{
				FluidContainer otherContainer = connections[connectionIndex];
				if (!container.HasRightToUpdate(otherContainer) || (container.Value < 0.0001f && otherContainer.Value < 0.0001f))
				{
					continue;
				}
				Fluid fluid = container.Fluid;
				Fluid otherFluid = otherContainer.Fluid;
				if (fluid != otherFluid)
				{
					if (fluid != null && otherFluid == null)
					{
						otherContainer.Fluid = fluid;
					}
					else
					{
						if (fluid != null || otherFluid == null)
						{
							continue;
						}
						fluid = (container.Fluid = otherFluid);
					}
				}
				if (fluid == null)
				{
					if (container.FlowRates[connectionIndex] != 0f)
					{
						Debug.LogWarning("Clearing flow rate on " + container.Name + " but was not 0: " + container.FlowRates[connectionIndex]);
					}
					container.FlowRates[connectionIndex] = 0f;
				}
				else
				{
					float flow = container.FlowRates[connectionIndex];
					flow += (otherContainer.Pressure - container.Pressure) * container.Fluid.FlowRateFactor;
					flow *= 1f - container.Fluid.Friction;
					flow = ((!(flow > 0f)) ? (0f - ClampFlow(container, otherContainer, 0f - flow)) : ClampFlow(otherContainer, container, flow));
					container.NextValue += flow;
					otherContainer.NextValue -= flow;
					container.FlowRates[connectionIndex] = flow;
				}
			}
		}
		for (int j = 0; j < containerCount; j++)
		{
			FluidContainer container2 = ContainersList[j];
			float newValue = container2.NextValue;
			if (newValue < 0f)
			{
				if (newValue < -0.001f)
				{
					Debug.LogError("Container " + container2.Name + " underflow: " + newValue + " vs " + container2.Max);
				}
				newValue = 0f;
			}
			if (newValue > container2.Max)
			{
				if (newValue > container2.Max + 0.001f)
				{
					Debug.LogError("Container " + container2.Name + " overflow: " + newValue + " vs " + container2.Max);
				}
				newValue = container2.Max;
			}
			if (container2.Fluid != null && newValue < 0.001f)
			{
				newValue = 0f;
				container2.Fluid = null;
			}
			if (container2.Fluid == null && newValue != 0f)
			{
				Debug.LogError("Container " + container2.Name + " has value but no fluid, v= " + newValue);
			}
			container2.Value = newValue;
			container2.NextValue = newValue;
		}
	}

	public static FluidNetwork InsertToNetwork(FluidContainer container, List<MapEntity.Fluids_LinkedContainer> connectedContainers)
	{
		if (container.Network != null)
		{
			throw new Exception("Container already has network");
		}
		HashSet<FluidNetwork> uniqueNetworks = new HashSet<FluidNetwork>();
		foreach (MapEntity.Fluids_LinkedContainer connection in connectedContainers)
		{
			container.Link(connection.Container);
			uniqueNetworks.Add(connection.Container.Network);
		}
		FluidNetwork[] involvedNetworks = uniqueNetworks.ToArray();
		if (involvedNetworks.Length == 0)
		{
			FluidNetwork network = new FluidNetwork();
			network.AddContainer(container);
			return network;
		}
		if (involvedNetworks.Length == 1)
		{
			FluidNetwork network2 = involvedNetworks[0];
			network2.AddContainer(container);
			return network2;
		}
		FluidNetwork target = involvedNetworks[0];
		for (int i = 1; i < involvedNetworks.Length; i++)
		{
			target.MergeWithDestroyOther(involvedNetworks[i]);
		}
		target.AddContainer(container);
		return target;
	}

	public static void RemoveFromNetwork(FluidContainer container)
	{
		container.Network.RemoveContainer(container);
	}

	protected void MergeWithDestroyOther(FluidNetwork other)
	{
		List<FluidContainer> otherContainers = other.ContainersList;
		int otherContainerCount = otherContainers.Count;
		for (int i = 0; i < otherContainerCount; i++)
		{
			otherContainers[i].Network = this;
		}
		Containers.UnionWith(other.Containers);
		ContainersList = Containers.ToList();
		other.Containers.Clear();
		other.ContainersList.Clear();
		other.UID = -1;
	}

	protected void AddContainer(FluidContainer container)
	{
		if (Containers.Contains(container))
		{
			throw new Exception("double container add on " + UID);
		}
		Containers.Add(container);
		ContainersList.Add(container);
		container.Network = this;
	}

	protected void RemoveContainer(FluidContainer container)
	{
		if (!Containers.Contains(container))
		{
			throw new Exception("Failed container remove on " + UID);
		}
		Containers.Remove(container);
		ContainersList.Remove(container);
		container.UnlinkAll();
		container.Network = null;
		Dictionary<FluidContainer, int> ContainerFlags = new Dictionary<FluidContainer, int>();
		int newNetworkCount = 0;
		int containerCount = ContainersList.Count;
		for (int i = 0; i < containerCount; i++)
		{
			if (FlagContainer(ContainersList[i], newNetworkCount))
			{
				newNetworkCount++;
			}
		}
		if (newNetworkCount <= 1)
		{
			return;
		}
		Containers.Clear();
		ContainersList.Clear();
		FluidNetwork[] newNetworks = new FluidNetwork[newNetworkCount];
		for (int j = 0; j < newNetworkCount; j++)
		{
			newNetworks[j] = new FluidNetwork();
		}
		foreach (KeyValuePair<FluidContainer, int> entry in ContainerFlags)
		{
			FluidContainer c = entry.Key;
			FluidNetwork network = newNetworks[entry.Value];
			c.Network = null;
			network.AddContainer(c);
		}
		bool FlagContainer(FluidContainer fluidContainer, int index)
		{
			if (ContainerFlags.ContainsKey(fluidContainer))
			{
				return false;
			}
			ContainerFlags[fluidContainer] = index;
			foreach (FluidContainer connection in fluidContainer.ConnectedContainers)
			{
				FlagContainer(connection, index);
			}
			return true;
		}
	}
}
