using System;
using UnityEngine;
using UnityEngine.Events;

public class HUDDialogStack : IHUDDialogStack, IDisposable
{
	[NonSerialized]
	public UnityEvent<GameObject, Action<HUDDialog>> InternalCallback;

	private bool disposed;

	public HUDDialogStack(UnityEvent<GameObject, Action<HUDDialog>> internalCallback)
	{
		InternalCallback = internalCallback;
	}

	public void Dispose()
	{
		disposed = true;
	}

	public T ShowUIDialog<T>() where T : HUDDialog
	{
		if (disposed)
		{
			throw new ObjectDisposedException("HUDDialogStack");
		}
		GameObject prefab = Resources.Load<GameObject>(typeof(T).Name + "Prefab");
		T result = null;
		InternalCallback.Invoke(prefab, delegate(HUDDialog dialog)
		{
			if ((!dialog) is T)
			{
				throw new Exception("Bad dialog: is not of type " + typeof(T).Name + " but " + dialog.GetType().Name);
			}
			result = dialog as T;
		});
		if (result == null)
		{
			Debug.LogError("No dialog handler for ShowDialog");
		}
		return result;
	}
}
