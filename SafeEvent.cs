public class SafeEvent : BaseSafeEvent<NoParamDelegate>
{
	public void Invoke()
	{
		InvokeInternal();
	}
}
public class SafeEvent<T> : BaseSafeEvent<OneParamDelegate<T>>
{
	public void Invoke(T value)
	{
		InvokeInternal(value);
	}
}
public class SafeEvent<T1, T2> : BaseSafeEvent<TwoParamDelegate<T1, T2>>
{
	public void Invoke(T1 value1, T2 value2)
	{
		InvokeInternal(value1, value2);
	}
}
