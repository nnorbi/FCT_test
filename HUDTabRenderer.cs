using System;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Dependency;
using Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDTabRenderer : HUDDebugPanelTab, IHUDDebugUpdateable
{
	public struct Instance
	{
		public static readonly Instance Default = new Instance
		{
			Distance = float.PositiveInfinity
		};

		public Mesh Mesh;

		public float Distance;

		public Material Material;

		public string StackTrace;

		public Matrix4x4 Transform;

		public Bounds BaseBounds;
	}

	[SerializeField]
	protected TMP_Text UIMeshName;

	[SerializeField]
	protected TMP_Text UIMaterialName;

	[SerializeField]
	protected TMP_Text UIStackTrace;

	[SerializeField]
	protected Button UIPipetteByBoundsButton;

	protected bool Pipetting;

	protected Ray CurrentRay;

	protected Instance BestMatch = Instance.Default;

	protected (MeshFilter, MeshRenderer)[] CachedSceneRenderers;

	void IHUDDebugUpdateable.OnUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
	}

	[Construct]
	private void Construct()
	{
		UIPipetteByBoundsButton.onClick.AddListener(delegate
		{
			Pipetting = true;
		});
		UIMeshName.text = "-";
		UIMaterialName.text = "-";
		UIStackTrace.text = "-";
		CachedSceneRenderers = (from x in UnityEngine.Object.FindObjectsOfType<MeshFilter>()
			select (x: x, x.GetComponent<MeshRenderer>())).ToArray();
	}

	protected void CheckMeshIntersectsRay(Matrix4x4 transformMatrix, Mesh mesh, Material material)
	{
		Matrix4x4 inverseMatrix = transformMatrix.inverse;
		Vector3 convertedSpaceRayOrigin = inverseMatrix.MultiplyPoint(CurrentRay.origin);
		Vector3 convertedSpaceRayDirection = inverseMatrix.MultiplyVector(CurrentRay.direction).normalized;
		Ray updatedRay = new Ray(convertedSpaceRayOrigin, convertedSpaceRayDirection);
		if (mesh.bounds.IntersectRay(updatedRay, out var distance) && distance < BestMatch.Distance)
		{
			BestMatch = new Instance
			{
				Distance = distance,
				Material = material,
				Mesh = mesh,
				StackTrace = Environment.StackTrace,
				Transform = transformMatrix,
				BaseBounds = mesh.bounds
			};
		}
	}

	protected void CollectData()
	{
		if (BestMatch.Mesh == null)
		{
			UIMeshName.text = "-";
			UIMaterialName.text = "-";
			UIStackTrace.text = "-";
		}
		else if (BestMatch.Transform.ValidTRS())
		{
			CommandBuilder draw = Draw.ingame;
			draw.PushMatrix(BestMatch.Transform);
			draw.WireBox(BestMatch.BaseBounds);
			draw.PopMatrix();
			UIMeshName.text = BestMatch.Mesh.name;
			UIMaterialName.text = BestMatch.Material.name;
			UIStackTrace.text = FilterMethodNamesFromStackTrace(BestMatch.StackTrace);
		}
	}

	protected static string FilterMethodNamesFromStackTrace(string stackTrace)
	{
		return string.Join(Environment.NewLine, Regex.Matches(stackTrace, "at ([^\\s]+)"));
	}

	protected void CheckGameObjectsIntersections()
	{
		(MeshFilter, MeshRenderer)[] cachedSceneRenderers = CachedSceneRenderers;
		for (int i = 0; i < cachedSceneRenderers.Length; i++)
		{
			var (meshFilter, meshRenderer) = cachedSceneRenderers[i];
			CheckMeshIntersectsRay(meshFilter.transform.localToWorldMatrix, meshFilter.mesh, meshRenderer.material);
		}
	}
}
