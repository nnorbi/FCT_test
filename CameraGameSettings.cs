public class CameraGameSettings : GameSettingsGroup
{
	public EnumGameSettingLegacy<int> ViewportSnapping = new EnumGameSettingLegacy<int>("viewport-snapping", "30", new DynamicEnumGameSetting<int>.Entry[5]
	{
		new DynamicEnumGameSetting<int>.Entry("off", 0),
		new DynamicEnumGameSetting<int>.Entry("15", 15),
		new DynamicEnumGameSetting<int>.Entry("30", 30),
		new DynamicEnumGameSetting<int>.Entry("45", 45),
		new DynamicEnumGameSetting<int>.Entry("90", 90)
	});

	public FloatGameSetting KeyboardCameraSpeed = new FloatGameSetting("keyboard-camera-speed", 1f, 0.01f, 5f);

	public FloatGameSetting MouseCameraDragSensitivityX = new FloatGameSetting("mouse-camera-drag-sensitivity-x", 1f, 0.01f, 5f);

	public FloatGameSetting MouseCameraDragSensitivityY = new FloatGameSetting("mouse-camera-drag-sensitivity-y", 1f, 0.01f, 5f);

	public BoolGameSetting MouseBorderPan = new BoolGameSetting("mouse-border-pan", defaultValue: true);

	public FloatGameSetting MouseBorderPanSpeed = new FloatGameSetting("mouse-border-pan-speed", 1f, 0.01f, 5f);

	public BoolGameSetting ConfineMouseCursor = new BoolGameSetting("confine-mouse-cursor", defaultValue: true);

	public BoolGameSetting InvertZoom = new BoolGameSetting("invert-zoom", defaultValue: false);

	public BoolGameSetting MoveCameraWithLayers = new BoolGameSetting("move-camera-with-layers", defaultValue: true);

	public FloatGameSetting ZoomSensitivity = new FloatGameSetting("zoom-sensitivity", 1f, 0.01f, 5f);

	public CameraGameSettings(bool saveOnChange)
		: base("camera-settings", saveOnChange)
	{
		Register(ViewportSnapping);
		Register(KeyboardCameraSpeed);
		Register(MouseCameraDragSensitivityX);
		Register(MouseCameraDragSensitivityY);
		Register(MouseBorderPan);
		Register(ConfineMouseCursor);
		Register(MouseBorderPanSpeed);
		Register(InvertZoom);
		Register(ZoomSensitivity);
		Register(MoveCameraWithLayers);
	}
}
