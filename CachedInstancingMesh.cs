#define UNITY_ASSERTIONS
using System;
using UnityEngine;

public class CachedInstancingMesh : IDisposable
{
	public readonly int InstancingID;

	private Mesh CachedMesh;

	private Func<Mesh> Generator;

	public bool Generated { get; private set; }

	public Mesh Mesh
	{
		get
		{
			if (!Generated)
			{
				Generate();
				Debug.Assert(Generated);
			}
			return CachedMesh;
		}
	}

	public static implicit operator Mesh(CachedInstancingMesh m)
	{
		return m.Mesh;
	}

	public CachedInstancingMesh(string identifier, Func<Mesh> generator)
	{
		Generated = false;
		Generator = generator;
		InstancingID = Shader.PropertyToID(identifier);
	}

	public void Dispose()
	{
		if (!(CachedMesh == null))
		{
			CachedMesh.Clear();
			UnityEngine.Object.Destroy(CachedMesh);
			CachedMesh = null;
		}
	}

	private void Generate()
	{
		Debug.Assert(!Generated, "already generated");
		CachedMesh = Generator();
		Generated = true;
	}
}
