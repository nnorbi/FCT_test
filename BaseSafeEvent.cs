using System;
using System.Collections.Generic;
using System.Linq;
using Core.Collections.Scoped;

public abstract class BaseSafeEvent<TDelegate> where TDelegate : Delegate
{
	private class Listener
	{
		public TDelegate Delegate;

		public bool PendingDeletion;

		public bool PendingAddition;
	}

	private List<Listener> Listeners = new List<Listener>();

	private bool DuringInvocation = false;

	protected void InvokeInternal(params object[] args)
	{
		if (DuringInvocation)
		{
			throw new Exception("Observable must not be changed during listener callback.");
		}
		DuringInvocation = true;
		try
		{
			DoInvokeListeners(args);
		}
		finally
		{
			DuringInvocation = false;
			using ScopedList<Listener> listenersToDelete = ScopedList<Listener>.Get();
			foreach (Listener listener in Listeners)
			{
				if (listener.PendingAddition)
				{
					listener.PendingAddition = false;
				}
				if (listener.PendingDeletion)
				{
					listenersToDelete.Add(listener);
				}
			}
			foreach (Listener listener2 in listenersToDelete)
			{
				Listeners.Remove(listener2);
			}
		}
	}

	private void DoInvokeListeners(object[] args)
	{
		using ScopedList<Listener> cachedListeners = ScopedList<Listener>.Get();
		cachedListeners.AddRange(Listeners);
		foreach (Listener listener in cachedListeners)
		{
			if (!listener.PendingDeletion && !listener.PendingAddition)
			{
				listener.Delegate.DynamicInvoke(args);
			}
		}
	}

	public void AddListener(TDelegate listenerDelegate)
	{
		if (Listeners.Any((Listener l) => l.Delegate == listenerDelegate))
		{
			throw new Exception("Can not add listener twice to Observable.");
		}
		Listeners.Add(new Listener
		{
			PendingAddition = DuringInvocation,
			Delegate = listenerDelegate,
			PendingDeletion = false
		});
	}

	public void RemoveListener(TDelegate listenerDelegate)
	{
		int index = Listeners.FindIndex((Listener l) => l.Delegate == listenerDelegate);
		if (index < 0)
		{
			throw new Exception("Can not remove listener that was never added.");
		}
		Listener handle = Listeners[index];
		if (handle.PendingDeletion)
		{
			throw new Exception("Can not remove listener that was already scheduled for deletion.");
		}
		if (DuringInvocation)
		{
			handle.PendingDeletion = true;
		}
		else
		{
			Listeners.RemoveAt(index);
		}
	}
}
