using System;
using System.Collections.Generic;

namespace Global.Core.Exceptions;

public class CollectingExceptionHandler : IExceptionHandler
{
	private readonly List<Exception> Exceptions = new List<Exception>();

	public IEnumerable<Exception> CollectedExceptions => Exceptions;

	public void HandleException(Exception exception)
	{
		Exceptions.Add(exception);
	}

	public void Clear()
	{
		Exceptions.Clear();
	}
}
