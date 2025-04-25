using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "ResearchableVideos", menuName = "Metadata/Research Videos")]
public class ResearchableVideos : ScriptableObject
{
	public EditorDict<MetaResearchable, VideoClip> VideoMap;

	[Header("Validation Only")]
	public MetaResearchTree ResearchTree;

	public bool TryGetVideoForResearch(IResearchableHandle handle, out VideoClip videoClip)
	{
		return VideoMap.CachedEntries.TryGetValue(handle.Meta, out videoClip);
	}
}
