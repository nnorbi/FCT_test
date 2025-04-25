public class ErrorTranslator : ITranslator
{
	public string LanguageTitle => "Error";

	public string LanguageCode => "en-US";

	public bool TryGetTranslation(string key, out string value)
	{
		value = "Error";
		return false;
	}
}
