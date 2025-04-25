using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public static class UnsafeExtensions
{
	public unsafe static T* Malloc<T>(int count, Allocator allocator = Allocator.TempJob) where T : unmanaged
	{
		return (T*)UnsafeUtility.Malloc(count * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<long>(), allocator);
	}
}
