public interface IPlayerAction
{
	PlayerActionMode Mode { get; }

	Player Executor { get; }

	bool TryExecute_INTERNAL(bool skipChecks_INTERNAL = false);

	bool IsPossible();

	IPlayerAction GetReverseActionIfPossible();
}
