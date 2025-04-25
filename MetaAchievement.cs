using UnityEngine;

[CreateAssetMenu(fileName = "Achievement", menuName = "Metadata/Achievement")]
public class MetaAchievement : ScriptableObject
{
	[SerializeField]
	public string InternalId;

	[SerializeField]
	public EditorDict<GameStore, string> AchievementId;

	[SerializeField]
	public int AchievementTarget;

	public string Title;

	public string Description;

	private void OnValidate()
	{
		Title = ("achievements." + InternalId + ".title").tr();
		Description = ("achievements." + InternalId + ".description").tr();
	}
}
