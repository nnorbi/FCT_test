using System;
using Global.Core.Exceptions;
using Newtonsoft.Json;

namespace Global.Core.Serialization.Json;

public abstract class ExceptionHandlingJsonConverter<T> : JsonConverter<T>, IExceptionHandler
{
	private readonly IExceptionHandler ExceptionHandler;

	protected ExceptionHandlingJsonConverter(IExceptionHandler exceptionHandler)
	{
		ExceptionHandler = exceptionHandler;
	}

	public void HandleException(Exception exception)
	{
		ExceptionHandler.HandleException(exception);
	}
}
