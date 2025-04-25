using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class IslandSimulator
{
	public static int UPS = 1000;

	public static double SECONDS_PER_TICK = 1.0 / (double)UPS;

	public static double MAX_DELTA_SECONDS = 0.5;

	protected static int MAX_TICKS_PER_FRAME = (int)((double)UPS * MAX_DELTA_SECONDS);

	protected TickOptions Options = new TickOptions();

	public double PendingSimulationTime = 0.0;

	public double SimulationTime_I { get; protected set; } = 0.0;

	public ulong SimulationTick_I { get; protected set; } = 0uL;

	public IEnumerable<TickOptions> Tick(IslandSimulationPayload payload, bool lowUPS)
	{
		double delta = payload.DeltaTime;
		delta *= (double)Singleton<GameCore>.G.SimulationSpeed.Speed;
		if (delta > MAX_DELTA_SECONDS)
		{
			delta = MAX_DELTA_SECONDS;
		}
		PendingSimulationTime += delta;
		double maxPending = 50.0 * MAX_DELTA_SECONDS;
		if (PendingSimulationTime > maxPending)
		{
			PendingSimulationTime = maxPending;
		}
		if (delta < 9.999999747378752E-05)
		{
			yield break;
		}
		int maxIterations = 10;
		bool budgetAllocated = !lowUPS;
		while (true)
		{
			int num = maxIterations - 1;
			maxIterations = num;
			if (num <= 0)
			{
				break;
			}
			int ticks = math.min((int)math.floor(PendingSimulationTime / SECONDS_PER_TICK), MAX_TICKS_PER_FRAME);
			if ((lowUPS && ticks != MAX_TICKS_PER_FRAME) || ticks <= 0 || (!budgetAllocated && !payload.BudgetAllocator()))
			{
				yield break;
			}
			budgetAllocated = true;
			yield return PerformTick(ticks, lowUPS);
		}
		if (maxIterations <= 1)
		{
			Debug.LogError("Out of simulation ticks, losing precision, factory will run slower than expected (remaining=" + PendingSimulationTime + ")");
			PendingSimulationTime = 0.0;
		}
	}

	protected TickOptions PerformTick(int ticks, bool lowUPS)
	{
		double actualDelta = (double)ticks * SECONDS_PER_TICK;
		PendingSimulationTime -= actualDelta;
		SimulationTick_I += (ulong)ticks;
		SimulationTime_I += actualDelta;
		Options.SimulationTime_G = Singleton<GameCore>.G.SimulationSpeed.SimulationTime_G;
		Options.DeltaTime = (float)actualDelta;
		Options.DeltaTicks_T = ticks;
		Options.LowUPS = lowUPS;
		return Options;
	}
}
