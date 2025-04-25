using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;

public class TrainManager
{
	protected struct SerializedNode
	{
		public int FromX;

		public int FromY;

		public int ToX;

		public int ToY;

		public char[] Colors;
	}

	protected struct SerializedTrain
	{
		public int PathFromX;

		public int PathFromY;

		public int PathToX;

		public int PathToY;

		public float Progress;

		public char Color;
	}

	protected class SerializedData
	{
		public SerializedNode[] Nodes;

		public SerializedTrain[] Trains;
	}

	public const string TOKEN_RENDER_TRAINS = "trains::render-trains";

	public const string TOKEN_RENDER_RAILS = "trains::render-rails";

	public static int RAIL_GRID_SIZE_G = 10;

	public GameMap Map;

	public Dictionary<int2, TrainRailNode> RailLookup_TG = new Dictionary<int2, TrainRailNode>();

	public List<Train> Trains = new List<Train>();

	public static float RAIL_HEIGHT_W => Singleton<GameCore>.G.Theme.BaseResources.RailHeight;

	public TrainManager(GameMap map)
	{
		Map = map;
	}

	public TrainRailNode GetNode_TG(int2 position_TG)
	{
		if (!TrainRailNode.IsValidCoordinate_TG(in position_TG))
		{
			int2 @int = position_TG;
			throw new Exception("Invalid train rail node: " + @int.ToString());
		}
		RailLookup_TG.TryGetValue(position_TG, out var node);
		return node;
	}

	public TrainRailNode GetOrCreateNode_TG(int2 position_TG)
	{
		if (!TrainRailNode.IsValidCoordinate_TG(in position_TG))
		{
			int2 @int = position_TG;
			throw new Exception("Invalid train rail node: " + @int.ToString());
		}
		RailLookup_TG.TryGetValue(position_TG, out var node);
		if (node == null)
		{
			return RailLookup_TG[position_TG] = new TrainRailNode(position_TG);
		}
		return node;
	}

	public Train PlaceTrainAt_DEBUG(int2 position_TG, MetaShapeColor color)
	{
		TrainRailNode node = GetNode_TG(position_TG);
		if (node == null)
		{
			return null;
		}
		if (node.Connections.Count == 0)
		{
			return null;
		}
		TrainSubPath connection = node.Connections[0];
		if (!connection.Colors.Contains(color))
		{
			return null;
		}
		if (connection.GetTrainsWithColor(color).Any())
		{
			return null;
		}
		Train train = new Train();
		train.Position = new Train.PositionLocator
		{
			Path = node.Connections[0],
			Progress = 0.5f
		};
		train.Color = color;
		connection.LinkTrain(train);
		Trains.Add(train);
		return train;
	}

	public void OnGameUpdate(float delta)
	{
		float speed = Singleton<GameCore>.G.SimulationSpeed.Speed;
		TickOptions options = new TickOptions
		{
			DeltaTicks_T = (int)((double)delta / IslandSimulator.SECONDS_PER_TICK * (double)speed),
			DeltaTime = delta * speed,
			SimulationTime_G = Singleton<GameCore>.G.SimulationSpeed.SimulationTime_G,
			LowUPS = false
		};
		foreach (Train train in Trains)
		{
			train.OnGameUpdate(options);
		}
	}

	public void OnGameDraw(FrameDrawOptions options)
	{
		if (options.DrawRails)
		{
			foreach (KeyValuePair<int2, TrainRailNode> entry in RailLookup_TG)
			{
				if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, entry.Value.Bounds))
				{
					entry.Value.OnGameDraw(options);
				}
			}
		}
		if (!options.DrawTrains)
		{
			return;
		}
		foreach (Train train in Trains)
		{
			train.OnGameDraw(options);
		}
	}

	public void Serialize(SavegameBlobWriter handle)
	{
		handle.Write("trains-wip.json.bin", delegate(BinaryStringLUTSerializationVisitor serializer)
		{
			List<SerializedNode> list = new List<SerializedNode>();
			foreach (KeyValuePair<int2, TrainRailNode> current in RailLookup_TG)
			{
				foreach (TrainSubPath current2 in current.Value.Connections)
				{
					if (current.Value.HasAuthorityOver(current2.To))
					{
						list.Add(new SerializedNode
						{
							FromX = current2.From.Position_TG.x,
							FromY = current2.From.Position_TG.y,
							ToX = current2.To.Position_TG.x,
							ToY = current2.To.Position_TG.y,
							Colors = current2.Colors.Select((MetaShapeColor c) => c.Code).ToArray()
						});
					}
				}
			}
			List<SerializedTrain> list2 = new List<SerializedTrain>();
			foreach (Train current3 in Trains)
			{
				list2.Add(new SerializedTrain
				{
					PathFromX = current3.Position.Path.From.Position_TG.x,
					PathFromY = current3.Position.Path.From.Position_TG.y,
					PathToX = current3.Position.Path.To.Position_TG.x,
					PathToY = current3.Position.Path.To.Position_TG.y,
					Progress = current3.Position.Progress,
					Color = current3.Color.Code
				});
			}
			SerializedData value = new SerializedData
			{
				Nodes = list.ToArray(),
				Trains = list2.ToArray()
			};
			string s = JsonConvert.SerializeObject(value);
			byte[] bytes = SavegameSerializerBase.ENCODING.GetBytes(s);
			serializer.WriteInt_4(bytes.Length);
			serializer.WriteBytesRaw(bytes);
		});
	}

	public void Deserialize(SavegameBlobReader handle)
	{
		if (handle.Metadata.Version < 1012)
		{
			Debug.Log("Trains:: Not deserializing old savegame");
			return;
		}
		handle.Read("trains-wip.json.bin", delegate(BinaryStringLUTSerializationVisitor handler)
		{
			int count = handler.ReadInt_4();
			byte[] bytes = handler.ReadBytesRaw(count);
			string value = SavegameSerializerBase.ENCODING.GetString(bytes);
			SerializedData serializedData = JsonConvert.DeserializeObject<SerializedData>(value);
			SerializedNode[] nodes = serializedData.Nodes;
			for (int i = 0; i < nodes.Length; i++)
			{
				SerializedNode serializedNode = nodes[i];
				TrainRailNode orCreateNode_TG = GetOrCreateNode_TG(new int2(serializedNode.FromX, serializedNode.FromY));
				TrainRailNode orCreateNode_TG2 = GetOrCreateNode_TG(new int2(serializedNode.ToX, serializedNode.ToY));
				char[] colors = serializedNode.Colors;
				foreach (char code in colors)
				{
					MetaShapeColor colorByCode = Singleton<GameCore>.G.Mode.GetColorByCode(code);
					if (orCreateNode_TG.IsConnectedTo(orCreateNode_TG2, colorByCode))
					{
						int2 position_TG = orCreateNode_TG.Position_TG;
						string text = position_TG.ToString();
						position_TG = orCreateNode_TG2.Position_TG;
						Debug.LogError("Duplicate rail connection between " + text + " / " + position_TG.ToString());
					}
					else
					{
						orCreateNode_TG.ConnectTo(orCreateNode_TG2, colorByCode);
						orCreateNode_TG2.ConnectTo(orCreateNode_TG, colorByCode);
					}
				}
			}
			SerializedTrain[] trains = serializedData.Trains;
			for (int k = 0; k < trains.Length; k++)
			{
				SerializedTrain serializedTrain = trains[k];
				TrainRailNode orCreateNode_TG3 = GetOrCreateNode_TG(new int2(serializedTrain.PathFromX, serializedTrain.PathFromY));
				TrainRailNode nodeTo = GetOrCreateNode_TG(new int2(serializedTrain.PathToX, serializedTrain.PathToY));
				MetaShapeColor colorByCode2 = Singleton<GameCore>.G.Mode.GetColorByCode(serializedTrain.Color);
				if (!orCreateNode_TG3.IsConnectedTo(nodeTo, colorByCode2))
				{
					string[] obj = new string[6] { "Train: no connection between ", null, null, null, null, null };
					int2 position_TG = orCreateNode_TG3.Position_TG;
					obj[1] = position_TG.ToString();
					obj[2] = " / ";
					position_TG = nodeTo.Position_TG;
					obj[3] = position_TG.ToString();
					obj[4] = " / ";
					obj[5] = serializedTrain.Color.ToString();
					Debug.LogError(string.Concat(obj));
				}
				else
				{
					TrainSubPath trainSubPath = orCreateNode_TG3.Connections.Find((TrainSubPath conn) => conn.To == nodeTo);
					if (trainSubPath.GetTrainsWithColor(colorByCode2).Any())
					{
						int2 position_TG = orCreateNode_TG3.Position_TG;
						Debug.LogWarning("Duplicate train with same color on connection " + position_TG.ToString());
					}
					else
					{
						Train train = new Train
						{
							Color = colorByCode2
						};
						trainSubPath.LinkTrain(train);
						if (serializedTrain.Progress > 1f)
						{
							Debug.LogWarning("Bad serialized train progress: " + serializedTrain.Progress);
						}
						train.Position = new Train.PositionLocator
						{
							Path = trainSubPath,
							Progress = math.min(serializedTrain.Progress, 1f)
						};
						Trains.Add(train);
					}
				}
			}
		});
	}
}
