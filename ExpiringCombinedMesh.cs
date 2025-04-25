using System;

public class ExpiringCombinedMesh : IExpiringResource
{
	protected CombinedMesh MeshInstance;

	public bool HasMesh { get; protected set; }

	public double LastUsed { get; protected set; } = -1000.0;

	public float ExpireAfter => 30f;

	public string Name => HasMesh ? ("combined-x-" + MeshInstance.MeshCount) : "<empty>";

	public void Hook_OnExpire()
	{
		Clear(unregister: false);
	}

	public void SetMesh(CombinedMesh mesh)
	{
		if (mesh != MeshInstance)
		{
			Clear();
			if (mesh != null)
			{
				HasMesh = true;
				MeshInstance = mesh;
				Singleton<GameCore>.G.ExpiringResources.Register(this);
			}
		}
	}

	public void Clear(bool unregister = true)
	{
		if (HasMesh)
		{
			HasMesh = false;
			MeshInstance.Clear();
			MeshInstance = null;
			if (unregister)
			{
				Singleton<GameCore>.G.ExpiringResources.Unregister(this);
			}
		}
	}

	public CombinedMesh GetMeshAndMarkUsed()
	{
		if (!HasMesh)
		{
			throw new Exception("Tried to get mesh but expiring mesh is not available anymore.");
		}
		LastUsed = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		return MeshInstance;
	}
}
