using DG.Tweening;

public interface IMainMenuStateControl
{
	Sequence SwitchToState<T>(object payload = null) where T : MainMenuState;

	void StartNewGame(GameModeConfig config);

	void ContinueExistingGame(SavegameReference entry);

	void ExitGame();
}
