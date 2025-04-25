using System;

namespace Global.Core.Serialization;

public interface ISerializer
{
	string Serialize(object value);

	T Deserialize<T>(string value);

	object Deserialize(string value, Type type);

	void Populate(string value, object target);
}
