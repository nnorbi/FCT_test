public interface IDrawer<T>
{
	void Draw(FrameDrawOptions draw, in T data);
}
