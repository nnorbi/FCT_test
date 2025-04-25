using System.Linq;

public class DebugTranslator : ITranslator
{
	public string LanguageTitle => "Debug";

	public string LanguageCode => "en-US";

	public bool TryGetTranslation(string key, out string value)
	{
		value = new string((char[])Enumerable.Repeat('*', key.Length));
		return true;
	}
}
