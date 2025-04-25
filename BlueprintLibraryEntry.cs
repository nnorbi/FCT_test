public class BlueprintLibraryEntry
{
	public string Title;

	public IBlueprint Blueprint;

	public BlueprintLibraryEntry(string title, IBlueprint blueprint)
	{
		Title = title;
		Blueprint = blueprint;
	}
}
