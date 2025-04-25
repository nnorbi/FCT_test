using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ActionModifyRails : PlayerAction
{
	public class DataPayload
	{
		public List<TrainRailPlacementPayload> Place = new List<TrainRailPlacementPayload>();

		public List<TrainRailDeletionPayload> Delete = new List<TrainRailDeletionPayload>();
	}

	protected DataPayload Data;

	public override PlayerActionMode Mode => PlayerActionMode.Blocking;

	public ActionModifyRails(GameMap map, Player executor, DataPayload data)
		: base(map, executor)
	{
		Data = data;
	}

	public override bool IsPossible()
	{
		TrainManager trains = Map.Trains;
		HashSet<Tuple<int2, int2, MetaShapeColor>> newConnectionsToAdd = new HashSet<Tuple<int2, int2, MetaShapeColor>>();
		foreach (TrainRailPlacementPayload payload in Data.Place)
		{
			if (payload.Color == null)
			{
				throw new Exception("Placement color must not be null");
			}
			if (!TrainRailNode.IsValidCoordinate_TG(in payload.Node1_TG))
			{
				int2 node1_TG = payload.Node1_TG;
				Debug.LogError("Bad Node1_TG: " + node1_TG.ToString());
				return false;
			}
			if (!TrainRailNode.IsValidCoordinate_TG(in payload.Node2_TG))
			{
				int2 node1_TG = payload.Node2_TG;
				Debug.LogError("Bad Node2_TG: " + node1_TG.ToString());
				return false;
			}
			if (!TrainRailNode.IsConnectionPossible(in payload.Node1_TG, in payload.Node2_TG))
			{
				int2 node1_TG = payload.Node1_TG;
				string text = node1_TG.ToString();
				node1_TG = payload.Node2_TG;
				Debug.LogError("Rail connection not possible: " + text + " <-> " + node1_TG.ToString());
				return false;
			}
			TrainRailNode node1 = trains.GetNode_TG(payload.Node1_TG);
			TrainRailNode node2 = trains.GetNode_TG(payload.Node2_TG);
			if (node1 != null && node2 != null)
			{
				if (node1.IsConnectedTo(node2, payload.Color) || node2.IsConnectedTo(node1, payload.Color))
				{
					int2 node1_TG = payload.Node1_TG;
					string text2 = node1_TG.ToString();
					node1_TG = payload.Node2_TG;
					Debug.LogError("Nodes already connected, to = node2 at " + text2 + " <-> " + node1_TG.ToString());
					return false;
				}
				if (newConnectionsToAdd.Contains(new Tuple<int2, int2, MetaShapeColor>(payload.Node1_TG, payload.Node2_TG, payload.Color)))
				{
					int2 node1_TG = payload.Node1_TG;
					string text3 = node1_TG.ToString();
					node1_TG = payload.Node2_TG;
					Debug.LogError("Duplicate place connection in single action not possible: " + text3 + " <-> " + node1_TG.ToString());
					return false;
				}
				newConnectionsToAdd.Add(new Tuple<int2, int2, MetaShapeColor>(payload.Node1_TG, payload.Node2_TG, payload.Color));
				newConnectionsToAdd.Add(new Tuple<int2, int2, MetaShapeColor>(payload.Node2_TG, payload.Node1_TG, payload.Color));
			}
		}
		HashSet<Tuple<int2, int2, MetaShapeColor>> connectionsToDelete = new HashSet<Tuple<int2, int2, MetaShapeColor>>();
		foreach (TrainRailDeletionPayload payload2 in Data.Delete)
		{
			if (payload2.Color == null)
			{
				throw new Exception("Deletion color must not be null");
			}
			TrainRailNode node3 = trains.GetNode_TG(payload2.Node1_TG);
			TrainRailNode node4 = trains.GetNode_TG(payload2.Node2_TG);
			if (node3 == null || node4 == null)
			{
				string[] obj = new string[7] { "Can not delete non existent connection between ", null, null, null, null, null, null };
				int2 node1_TG = payload2.Node1_TG;
				obj[1] = node1_TG.ToString();
				obj[2] = " and ";
				node1_TG = payload2.Node2_TG;
				obj[3] = node1_TG.ToString();
				obj[4] = " col ";
				obj[5] = payload2.Color.Code.ToString();
				obj[6] = " -> nodes don't exist";
				Debug.LogError(string.Concat(obj));
				return false;
			}
			if (!node3.IsConnectedTo(node4, payload2.Color) || !node4.IsConnectedTo(node3, payload2.Color))
			{
				string[] obj2 = new string[7] { "Can not delete non existent connection between ", null, null, null, null, null, null };
				int2 node1_TG = payload2.Node1_TG;
				obj2[1] = node1_TG.ToString();
				obj2[2] = " and ";
				node1_TG = payload2.Node2_TG;
				obj2[3] = node1_TG.ToString();
				obj2[4] = " col ";
				obj2[5] = payload2.Color.Code.ToString();
				obj2[6] = " -> no connection";
				Debug.LogError(string.Concat(obj2));
				return false;
			}
			if (connectionsToDelete.Contains(new Tuple<int2, int2, MetaShapeColor>(payload2.Node1_TG, payload2.Node2_TG, payload2.Color)))
			{
				string[] obj3 = new string[6] { "Duplicate delete connection in single action not possible: ", null, null, null, null, null };
				int2 node1_TG = payload2.Node1_TG;
				obj3[1] = node1_TG.ToString();
				obj3[2] = " <-> ";
				node1_TG = payload2.Node2_TG;
				obj3[3] = node1_TG.ToString();
				obj3[4] = " ";
				obj3[5] = payload2.Color.Code.ToString();
				Debug.LogError(string.Concat(obj3));
				return false;
			}
			connectionsToDelete.Add(new Tuple<int2, int2, MetaShapeColor>(payload2.Node1_TG, payload2.Node2_TG, payload2.Color));
			connectionsToDelete.Add(new Tuple<int2, int2, MetaShapeColor>(payload2.Node2_TG, payload2.Node1_TG, payload2.Color));
		}
		return true;
	}

	protected override void ExecuteInternal()
	{
		TrainManager trains = Map.Trains;
		foreach (TrainRailPlacementPayload payload in Data.Place)
		{
			TrainRailNode node1 = trains.GetOrCreateNode_TG(payload.Node1_TG);
			TrainRailNode node2 = trains.GetOrCreateNode_TG(payload.Node2_TG);
			node1.ConnectTo(node2, payload.Color);
			node2.ConnectTo(node1, payload.Color);
		}
		foreach (TrainRailDeletionPayload payload2 in Data.Delete)
		{
			TrainRailNode node3 = trains.GetOrCreateNode_TG(payload2.Node1_TG);
			TrainRailNode node4 = trains.GetOrCreateNode_TG(payload2.Node2_TG);
			TrainSubPath conn1 = node3.GetConnectionTo(node4, payload2.Color);
			TrainSubPath conn2 = node4.GetConnectionTo(node3, payload2.Color);
			Train[] array = conn1.GetTrainsWithColor(payload2.Color).ToArray();
			foreach (Train train in array)
			{
				train.DeregisterFromPath();
				trains.Trains.Remove(train);
			}
			Train[] array2 = conn2.GetTrainsWithColor(payload2.Color).ToArray();
			foreach (Train train2 in array2)
			{
				train2.DeregisterFromPath();
				trains.Trains.Remove(train2);
			}
			node3.DisconnectFrom(node4, payload2.Color);
			node4.DisconnectFrom(node3, payload2.Color);
		}
	}
}
