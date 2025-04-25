using System;
using UnityEngine;

public class ExpiringMesh : IExpiringResource
{
	public static string MESH_NAME_CACHED_INDICATOR = "^CACHED$";

	protected Mesh MeshInstance;

	public bool HasMesh { get; protected set; }

	public double LastUsed { get; protected set; } = -1000.0;

	public float ExpireAfter => 30f;

	public string Name => HasMesh ? MeshInstance.name : "<empty>";

	public void Hook_OnExpire()
	{
		Clear(unregister: false);
	}

	public void SetMesh(Mesh mesh)
	{
		if (mesh == MeshInstance)
		{
			return;
		}
		Clear();
		if (!(mesh == null))
		{
			if (mesh.name.Contains(MESH_NAME_CACHED_INDICATOR))
			{
				throw new Exception("Tried to create an Expiring Mesh, but its a shared, cached mesh that must not be cleared (" + mesh.name + ")");
			}
			HasMesh = true;
			MeshInstance = mesh;
			Singleton<GameCore>.G.ExpiringResources.Register(this);
		}
	}

	public void Clear(bool unregister = true)
	{
		if (HasMesh)
		{
			if (MeshInstance.name.Contains(MESH_NAME_CACHED_INDICATOR))
			{
				throw new Exception("Tried to call Clear() on Expiring Mesh, but its a shared, cached mesh that must not be cleared (" + MeshInstance.name + ")");
			}
			HasMesh = false;
			MeshInstance.Clear();
			UnityEngine.Object.Destroy(MeshInstance);
			MeshInstance = null;
			if (unregister)
			{
				Singleton<GameCore>.G.ExpiringResources.Unregister(this);
			}
		}
	}

	public Mesh GetMeshAndMarkUsed()
	{
		if (!HasMesh)
		{
			throw new Exception("Tried to get mesh but expiring mesh is not available anymore.");
		}
		LastUsed = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		return MeshInstance;
	}
}
