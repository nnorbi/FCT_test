using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Events;

public class PlayerWaypoints
{
	public class SerializedData
	{
		public PlayerWaypoint[] Waypoints = new PlayerWaypoint[0];
	}

	public UnityEvent<PlayerWaypoint> WaypointAdded = new UnityEvent<PlayerWaypoint>();

	public UnityEvent<PlayerWaypoint> WaypointChanged = new UnityEvent<PlayerWaypoint>();

	public UnityEvent<PlayerWaypoint> WaypointRemoved = new UnityEvent<PlayerWaypoint>();

	public PlayerWaypoint SavedPosition = null;

	public UnityEvent<PlayerWaypoint> SavedPositionChanged = new UnityEvent<PlayerWaypoint>();

	public Player Player;

	public List<PlayerWaypoint> Waypoints { get; protected set; } = new List<PlayerWaypoint>();

	public bool CanJumpBack => SavedPosition != null;

	public PlayerWaypoints(Player player)
	{
		Player = player;
	}

	public void Add(PlayerWaypointEditableData data)
	{
		PlayerWaypoint waypoint = CreateWaypointFromViewport();
		waypoint.Name = data.Name;
		waypoint.ShapeIconKey = data.ShapeIconKey;
		Waypoints.Add(waypoint);
		WaypointAdded.Invoke(waypoint);
	}

	protected PlayerWaypoint CreateWaypointFromViewport()
	{
		PlayerViewport viewport = Player.Viewport;
		return new PlayerWaypoint
		{
			UID = Guid.NewGuid().ToString("N"),
			PositionX = viewport.Position.x,
			PositionY = viewport.Position.y,
			Zoom = viewport.Zoom,
			Angle = viewport.Angle,
			Layer = viewport.Layer,
			RotationDegrees = viewport.RotationDegrees
		};
	}

	public void ChangeWaypoint(PlayerWaypoint waypoint, PlayerWaypointEditableData data)
	{
		waypoint.Name = data.Name;
		waypoint.ShapeIconKey = data.ShapeIconKey;
		WaypointChanged.Invoke(waypoint);
	}

	public void DeleteWaypoint(PlayerWaypoint waypoint)
	{
		if (Waypoints.Contains(waypoint))
		{
			Waypoints.Remove(waypoint);
			WaypointRemoved.Invoke(waypoint);
		}
	}

	public void JumpToWaypoint(PlayerWaypoint waypoint, bool storePosition = true)
	{
		if (storePosition)
		{
			SavedPosition = CreateWaypointFromViewport();
			SavedPositionChanged.Invoke(SavedPosition);
		}
		PlayerViewport viewport = Player.Viewport;
		viewport.Position = new float2(waypoint.PositionX, waypoint.PositionY);
		viewport.Zoom = waypoint.Zoom;
		viewport.Angle = waypoint.Angle;
		viewport.RotationDegrees = waypoint.RotationDegrees;
		viewport.Layer = waypoint.Layer;
		viewport.NewViewportLoaded.Invoke();
	}

	public void JumpBack()
	{
		if (SavedPosition != null)
		{
			JumpToWaypoint(SavedPosition, storePosition: false);
			SavedPosition = null;
			SavedPositionChanged.Invoke(SavedPosition);
		}
	}

	public void JumpToHub()
	{
		float3 hubPosition_W = Player.CurrentMap.HUBEntity.W_From_L(new float3(0f, 0f, 0f));
		JumpToWaypoint(new PlayerWaypoint
		{
			PositionX = hubPosition_W.x,
			PositionY = hubPosition_W.z,
			Angle = Player.Viewport.Angle,
			Zoom = math.max(25f, math.min(100f, Player.Viewport.Zoom)),
			Layer = 0,
			RotationDegrees = Player.Viewport.RotationDegrees
		});
	}

	public SerializedData Serialize()
	{
		return new SerializedData
		{
			Waypoints = Waypoints.ToArray()
		};
	}

	public void Deserialize(SerializedData data)
	{
		PlayerWaypoint[] oldWaypoints = Waypoints.ToArray();
		PlayerWaypoint[] array = oldWaypoints;
		foreach (PlayerWaypoint wp in array)
		{
			DeleteWaypoint(wp);
		}
		PlayerWaypoint[] waypoints = data.Waypoints;
		foreach (PlayerWaypoint wp2 in waypoints)
		{
			Waypoints.Add(wp2);
			WaypointAdded.Invoke(wp2);
		}
	}
}
