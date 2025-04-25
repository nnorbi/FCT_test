public interface IValueListGameSetting
{
	string[] AvailableValueIds { get; }

	int CurrentValueIndex { get; set; }

	string FormatValueId(string valueId);
}
