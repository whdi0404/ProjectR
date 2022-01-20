//여러 분류조건으로 찾아주는거?
public abstract class RObjectIndexer<TKey>
{
	public abstract TKey MakeKey(RObject obj);
}