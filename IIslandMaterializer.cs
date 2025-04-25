public interface IIslandMaterializer<in T>
{
	IslandDescriptor Materialize(T t);
}
