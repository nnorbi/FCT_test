using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Drawing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class HUDRailManagement : HUDPart
{
	[SerializeField]
	private RectTransform UIColorSelectorsParent;

	[SerializeField]
	private GameObject UIColorSelectorPrefab;

	protected Mesh GridMesh;

	protected bool Visible = false;

	protected List<ProceduralImage> ColorButtonImages = new List<ProceduralImage>();

	protected int2? StartPosition_TG;

	protected MetaShapeColor CurrentColor;

	[Construct]
	private void Construct()
	{
		GridMesh = GeometryHelpers.GetPlaneMesh_CACHED(new Color(1f, 1f, 1f, 1f));
		base.gameObject.SetActive(value: false);
		UIColorSelectorsParent.RemoveAllChildren();
		foreach (MetaShapeColor color in Singleton<GameCore>.G.Mode.ShapeColors)
		{
			GameObject obj = Object.Instantiate(UIColorSelectorPrefab, UIColorSelectorsParent);
			Button btn = obj.GetComponent<Button>();
			ProceduralImage img = obj.GetComponent<ProceduralImage>();
			img.color = color.Color;
			ColorButtonImages.Add(img);
			MetaShapeColor savedColor = color;
			HUDTheme.PrepareTheme(btn, HUDTheme.ButtonColorsIconOnly).onClick.AddListener(delegate
			{
				SelectColor(savedColor);
			});
		}
		SelectColor(Singleton<GameCore>.G.Mode.GetColorByCode('r'));
	}

	protected override void OnDispose()
	{
	}

	protected void SelectColor(MetaShapeColor color)
	{
		int index = Singleton<GameCore>.G.Mode.ShapeColors.IndexOf(color);
		for (int i = 0; i < ColorButtonImages.Count; i++)
		{
			ProceduralImage btn = ColorButtonImages[i];
			btn.gameObject.transform.localScale = new float3((i == index) ? 1.3f : 0.8f);
		}
		CurrentColor = color;
	}

	protected void Draw_PendingPlacement(FrameDrawOptions options, int2 from_TG, int2 to_TG)
	{
		List<TrainRailPlacementPayload> path = FindPath(from_TG, to_TG);
		if (path == null)
		{
			return;
		}
		foreach (TrainRailPlacementPayload node in path)
		{
			TrainSubPath.PathDescriptor description = TrainSubPath.GetPathDescriptor(node.Node1_TG, node.Node2_TG);
			TrainRailNode.DrawRailOverviewUX(options, node.Node1_TG, node.Node2_TG, description.Type, description.Direction, CurrentColor.Color, isPending: true);
		}
	}

	protected void LeaveTrainsMode()
	{
		Player.Viewport.Scope = GameScope.Islands;
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (Player.Viewport.Scope == GameScope.Trains)
		{
			Show();
		}
		else
		{
			Hide();
		}
		if (!Visible)
		{
			return;
		}
		if (!context.ConsumeToken("HUDPart$main_interaction"))
		{
			Debug.LogError("Rail management should always have main interaction");
			return;
		}
		if (context.ConsumeWasActivated("global.cancel"))
		{
			LeaveTrainsMode();
			return;
		}
		if (context.ConsumeWasActivated("main.toggle-rail-management"))
		{
			LeaveTrainsMode();
			return;
		}
		if (!Player.CurrentMap.InteractionMode.AllowRailManagement(Player))
		{
			LeaveTrainsMode();
			return;
		}
		drawOptions.AnalogUIRenderer.DrawMesh(GridMesh, FastMatrix.TranslateScale((float3)new Vector3(0f, TrainManager.RAIL_HEIGHT_W, 0f), new float3(100000f, 1f, 100000f)), drawOptions.Theme.BaseResources.UXRailPlacementOverviewPlaneMaterial, RenderCategory.Trains);
		using CommandBuilder draw = drawOptions.GetDebugDrawManager();
		ScreenUtils.TryGetRailCoordinateAtCursor(Player.Viewport, out var pos_TG);
		int2 pos_TG_Snapped = TrainRailNode.TG_TruncSnapped_From_TG(pos_TG);
		float3 pos_W = TrainRailNode.W_From_TG(in pos_TG_Snapped);
		TrainManager trains = Player.CurrentMap.Trains;
		TrainRailNode nodeFrom = trains.GetNode_TG(pos_TG_Snapped);
		if (nodeFrom != null)
		{
			using (List<TrainSubPath>.Enumerator enumerator = nodeFrom.Connections.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					TrainSubPath connection = enumerator.Current;
					draw.Arrow(TrainRailNode.W_From_TG(in connection.From.Position_TG), TrainRailNode.W_From_TG(in connection.To.Position_TG), new Color(1f, 1f, 1f));
				}
			}
			if (context.ConsumeWasActivated("rail-management.place-demo-train"))
			{
				if (trains.PlaceTrainAt_DEBUG(pos_TG_Snapped, CurrentColor) != null)
				{
					Globals.UISounds.PlayPlaceBuilding();
				}
				else
				{
					Globals.UISounds.PlayError();
				}
			}
			if (context.ConsumeIsActive("mass-selection.quick-delete-drag"))
			{
				List<TrainRailDeletionPayload> payload = (from conn in nodeFrom.Connections
					where conn.Colors.Contains(CurrentColor)
					select new TrainRailDeletionPayload
					{
						Node1_TG = conn.To.Position_TG,
						Node2_TG = conn.From.Position_TG,
						Color = CurrentColor
					}).ToList();
				ActionModifyRails action = new ActionModifyRails(Player.CurrentMap, Player, new ActionModifyRails.DataPayload
				{
					Delete = payload
				});
				Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action);
			}
		}
		if (StartPosition_TG.HasValue && context.ConsumeAllCheckOneActivated("global.cancel", "building-placement.cancel-placement"))
		{
			LeaveTrainsMode();
		}
		else if (StartPosition_TG.HasValue && !context.ConsumeIsActive("building-placement.confirm-placement"))
		{
			List<TrainRailPlacementPayload> path = FindPath(StartPosition_TG.Value, pos_TG_Snapped);
			if (path != null)
			{
				ActionModifyRails action2 = new ActionModifyRails(Player.CurrentMap, Player, new ActionModifyRails.DataPayload
				{
					Place = path
				});
				if (Singleton<GameCore>.G.PlayerActions.TryScheduleAction(action2))
				{
					Debug.Log("Place rails");
				}
			}
			StartPosition_TG = null;
		}
		else
		{
			if (!StartPosition_TG.HasValue && context.ConsumeWasActivated("building-placement.confirm-placement"))
			{
				StartPosition_TG = pos_TG_Snapped;
			}
			if (StartPosition_TG.HasValue)
			{
				draw.WireCylinder(TrainRailNode.W_From_TG(StartPosition_TG.Value), Vector3.up, 1f, 2f, new Color(0f, 1f, 1f));
				Draw_PendingPlacement(drawOptions, StartPosition_TG.Value, pos_TG_Snapped);
			}
			draw.WireCylinder(pos_W, Vector3.up, 1.5f, 2.5f, CurrentColor.Color);
			draw.SolidCircleXZ(pos_W + new float3(0f, 1f, 0f), 2f, CurrentColor.Color);
		}
	}

	protected void Show()
	{
		if (!Visible)
		{
			Visible = true;
			base.gameObject.SetActive(value: true);
		}
	}

	protected void Hide()
	{
		if (Visible)
		{
			StartPosition_TG = null;
			Visible = false;
			base.gameObject.SetActive(value: false);
		}
	}

	protected List<int2> ReconstructPath(Dictionary<int2, int2> cameFrom_TG, int2 current)
	{
		List<int2> total_path = new List<int2> { current };
		while (cameFrom_TG.ContainsKey(current))
		{
			current = cameFrom_TG[current];
			total_path.Insert(0, current);
		}
		return total_path;
	}

	protected List<int2> FindPathAStarRaw(int2 start, int2 goal_TG)
	{
		HashSet<int2> openSet = new HashSet<int2> { start };
		Dictionary<int2, int2> cameFrom = new Dictionary<int2, int2>();
		Dictionary<int2, float> gScore = new Dictionary<int2, float>();
		gScore[start] = 0f;
		Dictionary<int2, float> fScore = new Dictionary<int2, float>();
		fScore[start] = h(start);
		int maxIterations = 100000;
		while (openSet.Count > 0 && maxIterations-- > 0)
		{
			float lowestFScore = 0f;
			int2? lowestFScoreNode = null;
			foreach (int2 node in openSet)
			{
				float nodeFScore = fScore.GetValueOrDefault(node, 1E+10f);
				if (!lowestFScoreNode.HasValue || nodeFScore < lowestFScore)
				{
					lowestFScore = nodeFScore;
					lowestFScoreNode = node;
				}
			}
			int2 current = lowestFScoreNode.Value;
			if (current.Equals(goal_TG))
			{
				return ReconstructPath(cameFrom, current);
			}
			openSet.Remove(current);
			for (int dx = -2; dx <= 2; dx++)
			{
				for (int dy = -2; dy <= 2; dy++)
				{
					int2 neighbor = current + new int2(dx, dy);
					if (!TrainRailNode.IsConnectionPossible(in current, in neighbor))
					{
						continue;
					}
					float tentative_gScore = gScore.GetValueOrDefault(current, 1E+10f) + d(current, neighbor);
					if (tentative_gScore < gScore.GetValueOrDefault(neighbor, 1E+10f))
					{
						cameFrom[neighbor] = current;
						gScore[neighbor] = tentative_gScore;
						fScore[neighbor] = tentative_gScore + h(neighbor);
						if (!openSet.Contains(neighbor))
						{
							openSet.Add(neighbor);
						}
					}
				}
			}
		}
		return null;
		static float d(int2 a, int2 b)
		{
			if (a.x == b.x || a.y == b.y)
			{
				return 1f;
			}
			return math.distancesq(a, b) + 1000f;
		}
		float h(int2 @int)
		{
			return math.distancesq(@int, goal_TG);
		}
	}

	protected List<TrainRailPlacementPayload> FindPath(int2 from_TG, int2 to_TG)
	{
		TrainManager trains = Player.CurrentMap.Trains;
		List<int2> nodes = FindPathAStarRaw(from_TG, to_TG);
		if (nodes == null)
		{
			Debug.LogWarning("No path found");
			return null;
		}
		List<TrainRailPlacementPayload> result = new List<TrainRailPlacementPayload>();
		for (int i = 1; i < nodes.Count; i++)
		{
			int2 next = nodes[i];
			int2 prev = nodes[i - 1];
			if (!TrainRailNode.IsConnectionPossible(in prev, in next))
			{
				int2 @int = prev;
				string text = @int.ToString();
				@int = next;
				Debug.LogError("Bad pathfinding between " + text + " -> " + @int.ToString());
			}
			TrainRailNode node1 = trains.GetNode_TG(prev);
			TrainRailNode node2 = trains.GetNode_TG(next);
			if (node1 == null || node2 == null || !node1.IsConnectedTo(node2, CurrentColor))
			{
				result.Add(new TrainRailPlacementPayload
				{
					Node1_TG = prev,
					Node2_TG = next,
					Color = CurrentColor
				});
			}
		}
		return result;
	}
}
