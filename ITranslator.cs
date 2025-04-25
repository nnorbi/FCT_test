public interface ITranslator
{
	string LanguageTitle { get; }

	string LanguageCode { get; }

	bool TryGetTranslation(string key, out string value);
}
