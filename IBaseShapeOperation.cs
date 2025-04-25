public interface IBaseShapeOperation
{
	void GarbageCollect(float maxAgeSeconds, double now);

	void Clear();

	int GetCacheSize();
}
