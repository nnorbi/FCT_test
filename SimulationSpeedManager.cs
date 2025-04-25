using UnityEngine;

public class SimulationSpeedManager
{
	public float Speed { get; protected set; } = 1f;

	public double SimulationTime_G { get; protected set; } = 0.0;

	public double CurrentRealtime { get; protected set; } = 0.0;

	public bool Paused => Speed < 1E-05f;

	public SimulationSpeedManager()
	{
		SetSpeed(1f);
	}

	public void SetSpeed(float scale)
	{
		Speed = scale;
		Shader.SetGlobalFloat(GlobalShaderInputs.TimeScale, Speed);
	}

	public void SetPaused(bool paused)
	{
		SetSpeed((!paused) ? 1 : 0);
	}

	public void PerformTick(float dt)
	{
		CurrentRealtime = Time.realtimeSinceStartup;
		SimulationTime_G += dt * Speed;
		Shader.SetGlobalFloat(GlobalShaderInputs.GlobalSimulationTime, (float)(SimulationTime_G % 2048.0));
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("time.setspeed", new DebugConsole.FloatOption("speed", 0f, 25f), delegate(DebugConsole.CommandContext ctx)
		{
			SetSpeed(ctx.GetFloat(0));
		}, isCheat: true);
		console.Register("time.global-setspeed", new DebugConsole.FloatOption("speed", 0f, 25f), delegate(DebugConsole.CommandContext ctx)
		{
			Time.timeScale = ctx.GetFloat(0);
		}, isCheat: true);
	}
}
