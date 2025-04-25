using System.Collections.Generic;

public interface IHyperBeltInputManager
{
	IEnumerable<Checkpoint<GlobalChunkCoordinate>> Checkpoints { get; }

	void Update(InputDownstreamContext context, out HyperBeltInput input);

	void Reset();

	IEnumerable<HUDSidePanelHotkeyInfoData> GetActions();
}
