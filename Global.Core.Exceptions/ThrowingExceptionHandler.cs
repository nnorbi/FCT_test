using System;

namespace Global.Core.Exceptions;

public class ThrowingExceptionHandler : IExceptionHandler
{
	public void HandleException(Exception exception)
	{
		throw exception;
	}
}
