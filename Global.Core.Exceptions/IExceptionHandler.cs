using System;

namespace Global.Core.Exceptions;

public interface IExceptionHandler
{
	void HandleException(Exception exception);
}
