public static class Extension
{
	public static T ChangeType<T>(this object obj, T t)
	{
		return (T)obj;
	}
}
