using System;
using UnityEngine.Events;

public abstract class GameSetting : IEquatable<GameSetting>
{
	public readonly string Id;

	public UnityEvent Changed = new UnityEvent();

	public abstract bool IsModified { get; }

	protected string FullId { get; private set; }

	public string Title => ("menu.setting." + Id).tr();

	public virtual bool ShowInUI => true;

	public virtual bool RequiresRestart => false;

	protected GameSetting(string id)
	{
		Id = id;
	}

	public abstract bool Equals(GameSetting other);

	internal void SetParentId(string parentId)
	{
		FullId = "setting." + parentId + "." + Id;
	}

	public abstract void CopyFrom(GameSetting other);

	public abstract string GetValueText();

	public abstract void Write();

	public abstract void Read();

	public abstract bool TrySetFromString(string value);

	public virtual string GetHelpText()
	{
		return GetType().Name;
	}

	public abstract void ResetToDefault();
}
