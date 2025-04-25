public static class TypesCacheEditorTime
{
	public static AssemblyTypesCache TypesReflectionForCurrentAppDomain;

	static TypesCacheEditorTime()
	{
		TypesReflectionForCurrentAppDomain = new AssemblyTypesCache(ReflectionUtils.GetCurrentDomainUserAssemblies());
	}
}
