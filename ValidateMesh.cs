using System;

public class ValidateMesh : Attribute
{
	public int MaxVertexCount = 10000;

	public ValidateMesh()
	{
	}

	public ValidateMesh(int maxVertexCount)
	{
		MaxVertexCount = maxVertexCount;
	}
}
