using System;

[Serializable]
public class EditorClassIDSingleton<TBaseClass> : EditorClassID<TBaseClass>
{
	private TBaseClass _instance;

	public TBaseClass Instance
	{
		get
		{
			TBaseClass instance = _instance;
			return (instance != null) ? instance : (_instance = CreateInstance());
		}
	}

	public EditorClassIDSingleton(string defaultValue = "")
		: base(defaultValue)
	{
	}
}
