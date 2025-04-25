#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Core.Dependency;
using Core.Logging;
using Unity.Core.View;
using UnityEngine;

public abstract class HUDComponent : MonoBehaviour, IView, IDisposable
{
	private List<IView> Children = new List<IView>();

	private HashSet<IView> LoadedChildren = new HashSet<IView>();

	private IDependencyResolver DependencyResolver;

	private IPrefabViewInstanceProvider ViewInstanceProvider;

	private bool isConstructed;

	private bool isDisposed;

	protected Core.Logging.ILogger Logger { get; private set; }

	public void Dispose()
	{
		if (isDisposed || !isConstructed)
		{
			return;
		}
		try
		{
			OnDispose();
		}
		catch (Exception exception)
		{
			Logger.Exception?.LogException(exception);
		}
		foreach (IView childView in Children)
		{
			if (LoadedChildren.Remove(childView))
			{
				ViewInstanceProvider.ReleaseView(childView);
			}
			else if (childView is IDisposable disposableView)
			{
				disposableView.Dispose();
			}
		}
		Children.Clear();
		Debug.Assert(LoadedChildren.Count == 0);
		isDisposed = true;
	}

	public virtual void DoUpdate(InputDownstreamContext context)
	{
		if (!isConstructed)
		{
			throw new InvalidOperationException(base.name + " has not been constructed yet.");
		}
		if (isDisposed)
		{
			throw new ObjectDisposedException(base.name);
		}
		OnUpdate(context);
		foreach (IView child in Children)
		{
			if (child is HUDComponent updatableChild)
			{
				updatableChild.DoUpdate(context);
			}
		}
	}

	[Construct]
	private void Construct(IDependencyResolver dependencyResolver, Core.Logging.ILogger logger)
	{
		if (isConstructed)
		{
			throw new InvalidOperationException(base.name + " has already been constructed!");
		}
		if (isDisposed)
		{
			throw new ObjectDisposedException(base.name);
		}
		DependencyResolver = dependencyResolver ?? throw new ArgumentException("dependencyResolver");
		Logger = logger ?? throw new ArgumentException("logger");
		ViewInstanceProvider = new PrefabViewInstanceConstructor(dependencyResolver, logger);
		isConstructed = true;
	}

	protected void AddChildView<TViewInterface>(TViewInterface childView) where TViewInterface : UnityEngine.Object, IView
	{
		if (childView == null)
		{
			throw new ArgumentNullException("childView");
		}
		if (!isConstructed)
		{
			throw new InvalidOperationException(base.name + " has not been constructed yet.");
		}
		if (isDisposed)
		{
			throw new ObjectDisposedException(base.name);
		}
		if (Children.Contains(childView))
		{
			throw new Exception("Can not add child component twice.");
		}
		DependencyResolver.Inject(childView);
		Children.Add(childView);
		if (childView is IRunnableView runnableView)
		{
			runnableView.Run();
		}
	}

	protected virtual void OnUpdate(InputDownstreamContext context)
	{
	}

	protected PrefabRequestWithCallback<TViewInterface> RequestChildView<TViewInterface>(PrefabViewReference<TViewInterface> viewReference) where TViewInterface : MonoBehaviour, IView
	{
		if (!isConstructed)
		{
			throw new InvalidOperationException(base.name + " has not been constructed yet.");
		}
		if (isDisposed)
		{
			throw new ObjectDisposedException(base.name);
		}
		if (!viewReference.IsValid)
		{
			throw new ArgumentException("Requested child with invalid PrefabViewReference", "viewReference");
		}
		return ViewInstanceProvider.RequestView(viewReference).WithCallback(BeforeChildViewProvided);
	}

	private void BeforeChildViewProvided<TViewInterface>(TViewInterface childView) where TViewInterface : MonoBehaviour, IView
	{
		Children.Add(childView);
		LoadedChildren.Add(childView);
	}

	protected void ReleaseChildView(IView childView)
	{
		if (!isConstructed)
		{
			throw new InvalidOperationException(base.name + " has not been constructed yet.");
		}
		if (isDisposed)
		{
			throw new ObjectDisposedException(base.name);
		}
		if (childView == null)
		{
			throw new ArgumentNullException("childView");
		}
		if (!LoadedChildren.Remove(childView))
		{
			MonoBehaviour childObject = (MonoBehaviour)childView;
			throw new InvalidOperationException("Trying to release view " + childObject.name + " that is no child of HUDComponent " + base.name + ".");
		}
		bool removed = Children.Remove(childView);
		Debug.Assert(removed);
		ViewInstanceProvider.ReleaseView(childView);
	}

	protected abstract void OnDispose();
}
