public interface IPreloaderController
{
	void MoveToNextState();

	void CrashWithMessage(string message);

	void StopLoadingWithMessage(string message);
}
