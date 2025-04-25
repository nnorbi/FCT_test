public interface IDebugView
{
	string Name { get; }

	void OnGameDraw();

	void HandleInput(InputDownstreamContext inputs)
	{
	}
}
