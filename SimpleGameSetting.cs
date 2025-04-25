using System;

public abstract class SimpleGameSetting<T> : GameSetting where T : IEquatable<T>
{
	public T Value { get; private set; }

	protected T Default { get; }

	public override bool IsModified => !Value.Equals(Default);

	protected SimpleGameSetting(string id, T defaultValue)
		: base(id)
	{
		Value = (Default = defaultValue);
	}

	public override bool TrySetFromString(string value)
	{
		return false;
	}

	public override string GetValueText()
	{
		return Value.ToString();
	}

	public void SetValue(T value)
	{
		if (!value.Equals(Value))
		{
			Value = value;
			Changed.Invoke();
		}
	}

	public override bool Equals(GameSetting other)
	{
		if (!(other is SimpleGameSetting<T> otherSimpleSetting))
		{
			return false;
		}
		return Value.Equals(otherSimpleSetting.Value);
	}

	public override void ResetToDefault()
	{
		SetValue(Default);
	}

	public override void CopyFrom(GameSetting other)
	{
		if (!(other is SimpleGameSetting<T> otherSimpleSetting))
		{
			throw new Exception("Trying to assign incompatible setting");
		}
		if (!other.Equals(this))
		{
			SetValue(otherSimpleSetting.Value);
		}
	}
}
