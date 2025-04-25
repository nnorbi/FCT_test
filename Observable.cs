using System;
using System.Collections.Generic;
using JetBrains.Annotations;

public struct Observable<T> where T : IEquatable<T>
{
	private T _Value;

	public readonly SafeEvent<T> Changed;

	[CanBeNull]
	public T Value
	{
		get
		{
			return _Value;
		}
		set
		{
			if (!object.Equals(_Value, value))
			{
				_Value = value;
				Changed.Invoke(_Value);
			}
		}
	}

	public static bool operator ==(Observable<T> observable, T other)
	{
		return object.Equals(observable._Value, other);
	}

	public static bool operator !=(Observable<T> observable, T other)
	{
		return !object.Equals(observable._Value, other);
	}

	public static implicit operator T(Observable<T> observable)
	{
		return observable._Value;
	}

	public Observable(T defaultValue)
	{
		Changed = new SafeEvent<T>();
		_Value = defaultValue;
	}

	public bool Equals(Observable<T> other)
	{
		return EqualityComparer<T>.Default.Equals(_Value, other._Value);
	}

	public override bool Equals(object obj)
	{
		return obj is Observable<T> other && Equals(other);
	}

	public override int GetHashCode()
	{
		return EqualityComparer<T>.Default.GetHashCode(_Value);
	}

	public override string ToString()
	{
		object obj = _Value?.ToString();
		if (obj == null)
		{
			obj = "null";
		}
		return (string)obj;
	}
}
