using UnityEngine.Video;

public interface IResearchVideoMapper
{
	bool TryGetVideoForResearch(IResearchableHandle handle, out VideoClip clip);
}
