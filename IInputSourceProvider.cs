public interface IInputSourceProvider
{
	GameInputModeType InputMode { get; }

	void ChangeInputMode(GameInputModeType inputMode);
}
