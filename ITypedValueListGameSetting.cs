using System.Collections.Generic;

public interface ITypedValueListGameSetting<T>
{
	T Value { get; }

	IList<T> GenerateAvailableValues();

	void SetValue(T value);

	string FormatValue(T value);
}
