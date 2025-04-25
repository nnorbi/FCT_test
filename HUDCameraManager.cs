using System;
using Core.Dependency;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class HUDCameraManager : HUDPart
{
	public static readonly float MIN_ANGLE = 30f;

	public static readonly float MAX_ANGLE = 90f;

	public static readonly float MIN_ZOOM = 4f;

	public static readonly float MAX_ZOOM = 20000f;

	private static Plane VIRTUAL_CURSOR_PLANE = new Plane(Vector3.up, Vector3.zero);

	private Camera MainCamera;

	private Transform Parent;

	private float TargetAngle = 0f;

	private float2? TargetPosition;

	private float2 CurrentPosition;

	private float PanningSpeed = 0.5f;

	private float TargetRotation = 0f;

	private bool ZoomDirty = true;

	private bool AngleDirty = true;

	private bool RotationDirty = true;

	private bool PositionDirty = true;

	private PlayerViewport Viewport;

	private CameraGameSettings Settings;

	private float ZoomAdjustedMinAngle => MIN_ANGLE + math.saturate(math.pow(math.max(0f, Player.Viewport.Zoom - 1000f) / 7000f, 2f)) * 40f;

	[Construct]
	private void Construct(CameraGameSettings cameraSettings)
	{
		GameRenderingSetup.SetupCameraStack(Player, out MainCamera);
		Settings = cameraSettings;
		Parent = MainCamera.transform.parent;
		Viewport = Player.Viewport;
		CurrentPosition = Player.Viewport.Position;
		TargetAngle = Viewport.Angle;
		TargetRotation = Viewport.RotationDegrees;
		Viewport.TargetZoom = Viewport.Zoom;
		Viewport.PositionChanged.AddListener(delegate
		{
			PositionDirty = true;
		});
		Viewport.ZoomChanged.AddListener(delegate
		{
			ZoomDirty = true;
		});
		Viewport.AngleChanged.AddListener(delegate
		{
			AngleDirty = true;
		});
		Viewport.RotationChanged.AddListener(delegate
		{
			RotationDirty = true;
		});
		Viewport.MainCamera.backgroundColor = new Color(0f, 0f, 0f);
		Viewport.NewViewportLoaded.AddListener(OnNewViewportLoaded);
		UpdateCameraOnZoomOrAngleChange();
		InitCommands();
	}

	protected override void OnDispose()
	{
	}

	private void OnNewViewportLoaded()
	{
		Viewport.TargetZoom = Viewport.Zoom;
		TargetAngle = Viewport.Angle;
		TargetPosition = Viewport.Position;
		CurrentPosition = Viewport.Position;
		TargetRotation = Viewport.RotationDegrees;
	}

	private void RotateCamera(int offset)
	{
		int amount = Player.Viewport.SnappingDegrees;
		if (amount == 0)
		{
			amount = 15;
		}
		TargetRotation += offset * amount;
	}

	private void ZoomCamera(float amount)
	{
		Viewport.TargetZoom = math.clamp(Viewport.TargetZoom + amount * Viewport.TargetZoom, MIN_ZOOM, MAX_ZOOM);
	}

	private void ApplyAngleDelta(float amount)
	{
		TargetAngle = math.clamp(TargetAngle - amount, ZoomAdjustedMinAngle, MAX_ANGLE);
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		float deltaTime = math.min(0.2f, Time.deltaTime);
		Update_KeyBasedMovement(context, deltaTime);
		Update_AngleMovement(context);
		Update_MouseMovement(context);
		Update_ZoomInputs(context);
		Update_ScreenPanning(context, deltaTime);
		Update_ApplyAngleAndZoom(context, deltaTime);
		Update_ApplyPositionSmoothing(context, deltaTime);
		Update_ApplyDirtyChangesToCamera();
		Update_MouseShaderParams();
	}

	private void Update_KeyBasedMovement(InputDownstreamContext context, float deltaTime)
	{
		float moveRight = context.ConsumeAsAxis("camera.move-right");
		float moveLeft = context.ConsumeAsAxis("camera.move-left") * -1f;
		float moveUp = context.ConsumeAsAxis("camera.move-up");
		float moveDown = context.ConsumeAsAxis("camera.move-down") * -1f;
		float2 movement = new float2(moveRight + moveLeft, moveUp + moveDown);
		ApplyMovementVector(movement, deltaTime);
	}

	private void ApplyMovementVector(float2 movement, float deltaTime)
	{
		if (math.length(movement) > 0f && !TargetPosition.HasValue)
		{
			float2 speed = math.normalize(movement) * PanningSpeed * Settings.KeyboardCameraSpeed;
			speed *= Viewport.Zoom;
			CurrentPosition += Grid.Rotate(speed * math.min(1f, deltaTime * 2f), 0f - Viewport.RotationDegrees);
		}
	}

	private void Update_AngleMovement(InputDownstreamContext context)
	{
		if (context.ConsumeIsActive("camera.angle-drag-modifier"))
		{
			if (context.HasMouseDelta())
			{
				float2 delta = context.ConsumeMouseDelta() / new float2(Screen.width, Screen.height) * 1920f;
				ApplyAngleDelta(delta.y * 0.55f * (float)Settings.MouseCameraDragSensitivityY);
				TargetRotation += delta.x * 0.2f * (float)Settings.MouseCameraDragSensitivityX;
			}
		}
		else
		{
			int snapping = Viewport.SnappingDegrees;
			if ((float)snapping > 0f)
			{
				TargetRotation = math.round(TargetRotation / (float)snapping) * (float)snapping;
			}
		}
		float angleDragUp = context.ConsumeAsAxis("camera.angle-up");
		float angleDragDown = context.ConsumeAsAxis("camera.angle-down");
		float angleDrag = angleDragUp + angleDragDown * -1f;
		if (math.abs(angleDrag) > 0.01f)
		{
			ApplyAngleDelta(angleDrag);
		}
		if (context.ConsumeWasActivated("camera.rotate-cw"))
		{
			RotateCamera(1);
		}
		if (context.ConsumeWasActivated("camera.rotate-ccw"))
		{
			RotateCamera(-1);
		}
	}

	private void Update_MouseMovement(InputDownstreamContext context)
	{
		if (context.ConsumeIsActive("camera.mouse-drag-modifier") && context.HasMouseDelta())
		{
			float2 delta = context.ConsumeMouseDelta();
			if (!TargetPosition.HasValue)
			{
				float2 newPos = context.MousePosition;
				float2 oldPos = newPos - delta * new float2(1f, math.sin(math.radians(Viewport.Angle)));
				float2 centerWorld = GetCursorPointOnVirtualPlane(newPos);
				float2 centerWorldNew = GetCursorPointOnVirtualPlane(oldPos);
				CurrentPosition += centerWorldNew - centerWorld;
			}
		}
	}

	private void Update_ZoomInputs(InputDownstreamContext context)
	{
		if (context.HasWheelDelta())
		{
			float deltaZ = context.ConsumeWheelDelta();
			if ((bool)Settings.InvertZoom)
			{
				deltaZ *= -1f;
			}
			deltaZ *= (float)Settings.ZoomSensitivity;
			ZoomCamera(deltaZ * 0.03f);
		}
		float zoomAxisDelta = context.ConsumeAsAxis("camera.zoom-in") + context.ConsumeAsAxis("camera.zoom-out") * -1f;
		if (math.abs(zoomAxisDelta) > 0.01f)
		{
			ZoomCamera(zoomAxisDelta * 0.03f);
		}
	}

	private void Update_ScreenPanning(InputDownstreamContext context, float deltaTime)
	{
		if (!Settings.MouseBorderPan || !context.IsTokenAvailable("HUDPart$confine_cursor") || TargetPosition.HasValue)
		{
			return;
		}
		float2 mousePos = context.MousePosition;
		float panArea = 5f;
		int panX = 0;
		int panY = 0;
		if (!(mousePos.x < 0f) && !(mousePos.y < 0f) && !(mousePos.x > (float)(Screen.width + 1)) && !(mousePos.y > (float)(Screen.height + 1)))
		{
			if (mousePos.x < panArea)
			{
				panX = -1;
			}
			else if (mousePos.x > (float)Screen.width - panArea)
			{
				panX = 1;
			}
			if (mousePos.y < panArea)
			{
				panY = -1;
			}
			else if (mousePos.y > (float)Screen.height - panArea)
			{
				panY = 1;
			}
			float2 movement = new float2(panX, panY) * Settings.MouseBorderPanSpeed;
			ApplyMovementVector(movement, deltaTime);
		}
	}

	private void Update_ApplyAngleAndZoom(InputDownstreamContext context, float deltaTime)
	{
		if (math.abs(TargetAngle - Viewport.Angle) > 0.0001f || math.abs(Viewport.TargetZoom - Viewport.Zoom) > 0.0001f)
		{
			float fade = math.min(1f, deltaTime * 15f);
			Viewport.Angle = math.lerp(Viewport.Angle, TargetAngle, fade);
			Viewport.Zoom = math.lerp(Viewport.Zoom, Viewport.TargetZoom, fade);
		}
		Viewport.Zoom = math.clamp(Viewport.Zoom, MIN_ZOOM, MAX_ZOOM);
		if (math.abs(Viewport.RotationDegrees - TargetRotation) > 0.001f)
		{
			Viewport.RotationDegrees = math.lerp(Viewport.RotationDegrees, TargetRotation, math.saturate(deltaTime * 12f));
		}
	}

	private void Update_ApplyPositionSmoothing(InputDownstreamContext context, float deltaTime)
	{
		if (TargetPosition.HasValue)
		{
			float2 currentPos = CurrentPosition;
			float2 delta = new float2(currentPos.x, currentPos.y) - CurrentPosition;
			CurrentPosition -= math.min(1f, deltaTime * 6f) * delta;
			if (math.length(delta) < 0.01f)
			{
				TargetPosition = null;
			}
		}
		if (math.distancesq(CurrentPosition, Viewport.Position) > 0.001f)
		{
			Viewport.Position = math.lerp(Viewport.Position, CurrentPosition, math.saturate(Time.unscaledDeltaTime * 12f));
		}
	}

	private void Update_ApplyDirtyChangesToCamera()
	{
		if (ZoomDirty || AngleDirty)
		{
			UpdateCameraOnZoomOrAngleChange();
			ZoomDirty = false;
			AngleDirty = false;
		}
		if (RotationDirty)
		{
			Parent.localRotation = Quaternion.Euler(0f, Viewport.RotationDegrees, 0f);
			RotationDirty = false;
		}
		if (PositionDirty)
		{
			float height = Player.Viewport.Height;
			if (!Settings.MoveCameraWithLayers)
			{
				height = 0f;
			}
			Parent.position = new float3(Viewport.Position.x, height, Viewport.Position.y);
			PositionDirty = false;
		}
	}

	private float2 GetCursorPointOnVirtualPlane(float2 pos)
	{
		pos = math.clamp(pos, new float2(0), new float2(Screen.width, Screen.height));
		Ray mouseRay = MainCamera.ScreenPointToRay(new float3(pos, 0f));
		if (!VIRTUAL_CURSOR_PLANE.Raycast(mouseRay, out var enter))
		{
			Debug.LogWarning("VirtualCameraPlane no Intersection");
			return new float2(0);
		}
		return ((float3)mouseRay.GetPoint(enter)).xz;
	}

	private void Update_MouseShaderParams()
	{
		float3 cursorTile_W = new float3(1E+20);
		Plane plane = ScreenUtils.GetPlaneForTileLayer(Player.Viewport.Height);
		Ray mouseRay = Player.Viewport.ScreenCoordinateToRay(Player.Viewport.CursorScreenPosition);
		if (plane.Raycast(mouseRay, out var enter))
		{
			cursorTile_W = mouseRay.GetPoint(enter);
		}
		Shader.SetGlobalVector(GlobalShaderInputs.CursorWorldPos, (Vector3)cursorTile_W);
	}

	private void ClampAngle()
	{
	}

	private void UpdateCameraOnZoomOrAngleChange()
	{
		float baseAngle = Viewport.Angle;
		float zoom = Viewport.Zoom;
		float angle = baseAngle;
		float minAngle = ZoomAdjustedMinAngle;
		angle = math.max(minAngle, angle);
		bool flag = false;
		ApplyCameraTransform(angle, zoom);
		Shader.SetGlobalFloat(GlobalShaderInputs.Zoom, Viewport.Zoom);
		Shader.SetGlobalFloat(GlobalShaderInputs.CameraAngle, Viewport.Angle);
	}

	private void ApplyCameraTransform(float angle, float zoom)
	{
		float height = math.sin(math.radians(angle)) * zoom;
		Transform camTransform = MainCamera.transform;
		camTransform.localPosition = new Vector3(0f, height, (0f - math.cos(math.radians(angle))) * zoom);
		camTransform.localRotation = Quaternion.Euler(angle, 0f, 0f);
	}

	private void InitCommands()
	{
		Singleton<GameCore>.G.Console.Register("camera.set-pan-speed", new DebugConsole.FloatOption("speed", 0f, 100f), delegate(DebugConsole.CommandContext ctx)
		{
			PanningSpeed = ctx.GetFloat(0);
		});
		Singleton<GameCore>.G.Console.Register("camera.move-to", new DebugConsole.IntOption("x"), new DebugConsole.IntOption("y"), delegate(DebugConsole.CommandContext ctx)
		{
			Viewport.Position = (CurrentPosition = new float2(ctx.GetInt(0), ctx.GetInt(1)));
		});
		Singleton<GameCore>.G.Console.Register("camera.info", delegate(DebugConsole.CommandContext ctx)
		{
			Camera mainCamera = Player.Viewport.MainCamera;
			ctx.Output("Camera pos = " + mainCamera.transform.position.ToString() + " rotation = " + mainCamera.transform.eulerAngles.ToString() + " viewport = \nZOOM:" + Player.Viewport.Zoom + "\nANGLE:" + Player.Viewport.Angle + "\nHEIGHT:" + Player.Viewport.Height + "\nROT:" + Player.Viewport.RotationDegrees + "\nPOS: " + Player.Viewport.Position.ToString());
			Debug.Log("Camera pos = " + mainCamera.transform.position.ToString() + " rotation = " + mainCamera.transform.rotation.eulerAngles.ToString());
		});
	}
}
