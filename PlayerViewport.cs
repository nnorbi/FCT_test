using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerViewport
{
	[Serializable]
	public class SerializedData
	{
		public float PositionX;

		public float PositionY;

		public float Zoom;

		public float RotationDegrees;

		public float Angle;

		public short Layer;

		public bool ShowAllLayers;
	}

	[NonSerialized]
	public Camera MainCamera;

	[NonSerialized]
	public Camera TransparentCamera;

	[NonSerialized]
	public UnityEvent PositionChanged = new UnityEvent();

	[NonSerialized]
	public UnityEvent ZoomChanged = new UnityEvent();

	[NonSerialized]
	public UnityEvent AngleChanged = new UnityEvent();

	[NonSerialized]
	public UnityEvent RotationChanged = new UnityEvent();

	[NonSerialized]
	public UnityEvent LayerChanged = new UnityEvent();

	[NonSerialized]
	public UnityEvent ShowAllLayersChanged = new UnityEvent();

	[NonSerialized]
	public UnityEvent ScopeChanged = new UnityEvent();

	[NonSerialized]
	public UnityEvent NewViewportLoaded = new UnityEvent();

	protected float2 _Position = float2.zero;

	protected float _Height = 0f;

	protected float _Zoom = 10f;

	protected float _TargetZoom = 10f;

	protected float _Angle = 60f;

	protected float _RotationDegrees = 0f;

	protected short _Layer = 0;

	protected bool _ShowAllLayers = true;

	protected GameScope _Scope = GameScope.Buildings;

	public float Zoom
	{
		get
		{
			return _Zoom;
		}
		set
		{
			if (math.abs(value - _Zoom) > 0.001f)
			{
				_Zoom = value;
				ZoomChanged.Invoke();
			}
		}
	}

	public float TargetZoom
	{
		get
		{
			return _TargetZoom;
		}
		set
		{
			if (math.abs(value - _TargetZoom) > 0.001f)
			{
				_TargetZoom = value;
				ZoomChanged.Invoke();
			}
		}
	}

	public float2 Position
	{
		get
		{
			return _Position;
		}
		set
		{
			if (math.distance(value, _Position) > 0.001f)
			{
				_Position = value;
				PositionChanged.Invoke();
			}
		}
	}

	public float Height
	{
		get
		{
			return _Height;
		}
		set
		{
			if (math.abs(value - _Height) > 0.001f)
			{
				_Height = value;
				PositionChanged.Invoke();
			}
		}
	}

	public float2 CursorScreenPosition => math.clamp(Input.mousePosition, new float3(0), new float3(Screen.width, Screen.height, 0f)).xy;

	public Ray CursorRay => ScreenCoordinateToRay(CursorScreenPosition);

	public float Angle
	{
		get
		{
			return _Angle;
		}
		set
		{
			if (math.abs(value - _Angle) > 0.001f)
			{
				_Angle = value;
				AngleChanged.Invoke();
			}
		}
	}

	public Grid.Direction PrimaryDirection => (Grid.Direction)FastMath.SafeMod((int)math.round(_RotationDegrees / 90f), 4);

	public float RotationDegrees
	{
		get
		{
			return _RotationDegrees;
		}
		set
		{
			if (math.abs(value - _RotationDegrees) > 0.001f)
			{
				_RotationDegrees = value;
				RotationChanged.Invoke();
			}
		}
	}

	public short Layer
	{
		get
		{
			return _Layer;
		}
		set
		{
			if (_Layer != value)
			{
				_Layer = value;
				LayerChanged.Invoke();
			}
		}
	}

	public bool ShowAllLayers
	{
		get
		{
			return _ShowAllLayers;
		}
		set
		{
			if (_ShowAllLayers != value)
			{
				_ShowAllLayers = value;
				ShowAllLayersChanged.Invoke();
			}
		}
	}

	public GameScope Scope
	{
		get
		{
			return _Scope;
		}
		set
		{
			if (_Scope != value)
			{
				_Scope = value;
				ScopeChanged.Invoke();
			}
		}
	}

	public int SnappingDegrees => Globals.Settings.Camera.ViewportSnapping.Current.Value;

	public Ray ScreenCoordinateToRay(float2 screenCoordinate)
	{
		return MainCamera.ScreenPointToRay(new float3(screenCoordinate.xy, 0f));
	}

	public PlayerViewport Clone()
	{
		PlayerViewport result = new PlayerViewport();
		result.CopyFrom(this);
		return result;
	}

	public SerializedData Serialize()
	{
		return new SerializedData
		{
			PositionX = _Position.x,
			PositionY = _Position.y,
			Zoom = _Zoom,
			Angle = _Angle,
			Layer = _Layer,
			ShowAllLayers = _ShowAllLayers,
			RotationDegrees = _RotationDegrees % 360f
		};
	}

	public void Deserialize(SerializedData data)
	{
		Position = new float2(data.PositionX, data.PositionY);
		Height = data.Layer;
		Zoom = data.Zoom;
		Angle = data.Angle;
		RotationDegrees = data.RotationDegrees;
		Layer = data.Layer;
		ShowAllLayers = data.ShowAllLayers;
		TargetZoom = _Zoom;
		NewViewportLoaded.Invoke();
	}

	public void CopyFrom(PlayerViewport other)
	{
		Position = other._Position;
		Height = other._Height;
		Zoom = other._Zoom;
		Angle = other._Angle;
		RotationDegrees = other._RotationDegrees;
		Layer = other._Layer;
		ShowAllLayers = other._ShowAllLayers;
		Scope = other._Scope;
		TargetZoom = _Zoom;
		NewViewportLoaded.Invoke();
	}
}
