public interface IBenchmarkConfiguration
{
	string Title { get; }

	string[] Settings { get; }

	GraphicSettings RequiredGraphicSettings();
}
