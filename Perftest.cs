using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

public static class Perftest
{
	private static void Bench(DebugConsole.CommandContext ctx, string name, Action<int> v1, Action<int> v2)
	{
		UnityEngine.Debug.Log("Benchmark " + name);
		long ellapsedV1 = 0L;
		long ellapsedV2 = 0L;
		for (int i = 0; i < 20; i++)
		{
			Stopwatch watchV1 = Stopwatch.StartNew();
			v1(50000);
			watchV1.Stop();
			ellapsedV1 += watchV1.ElapsedMilliseconds;
			Stopwatch watchV2 = Stopwatch.StartNew();
			v2(50000);
			watchV2.Stop();
			ellapsedV2 += watchV2.ElapsedMilliseconds;
		}
		ctx.Output(name + " -> V1: " + ellapsedV1 + "ms, V2: " + ellapsedV2 + "ms");
	}

	public static void Run(DebugConsole.CommandContext ctx)
	{
		int _flag = 0;
		Bench(ctx, "float4x4.TRS vs Matrix4x4.TRS", delegate(int iter)
		{
			for (int i = 0; i < iter; i++)
			{
				if (float4x4.TRS(new float3(0.2f, 0.2f, 0.2f), Quaternion.Euler(0f, 90f, 0f), new float3(0f, 0f, 0f)).c0.x == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < iter; i++)
			{
				if (Matrix4x4.TRS(new float3(0.2f, 0.2f, 0.2f), Quaternion.Euler(0f, 90f, 0f), new float3(0f, 0f, 0f)).m00 == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "float4x4.TRS vs Matrix4x4.TRS with Vector3", delegate(int iter)
		{
			for (int i = 0; i < iter; i++)
			{
				if (float4x4.TRS(new float3(0.2f, 0.2f, 0.2f), Quaternion.Euler(0f, 90f, 0f), new float3(0f, 0f, 0f)).c0.x == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < iter; i++)
			{
				if (Matrix4x4.TRS(new Vector3(0.2f, 0.2f, 0.2f), Quaternion.Euler(0f, 90f, 0f), new Vector3(0f, 0f, 0f)).m00 == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "Matrix4x4.Translate vs custom (1)", delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				if (Matrix4x4.Translate(new float3(1f, 2f, 3f)).m00 == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				float3 @float = new float3(1f, 2f, 3f);
				if (new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(@float.x, @float.y, @float.z, 1f)).m00 == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "Matrix4x4.Translate (float3) vs Grid.Translate", delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				if (Matrix4x4.Translate(new float3(1f, 2f, 3f)).m00 == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				if (FastMatrix.Translate(new float3(1f, 2f, 3f)).m00 == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "Matrix4x4.Translate (Vector3) vs Grid.Translate", delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				if (Matrix4x4.Translate(new Vector3(1f, 2f, 3f)).m00 == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				if (FastMatrix.Translate(new float3(1f, 2f, 3f)).m00 == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "custom (1) vs custom (2)", delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				float3 @float = new float3(1f, 2f, 3f);
				if (new Matrix4x4(new Vector4(1f, 0f, 0f, 0f), new Vector4(0f, 1f, 0f, 0f), new Vector4(0f, 0f, 1f, 0f), new Vector4(@float.x, @float.y, @float.z, 1f)).m00 == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				float3 @float = new float3(1f, 2f, 3f);
				if (new Matrix4x4
				{
					m00 = 1f,
					m01 = 0f,
					m02 = 0f,
					m03 = @float.x,
					m10 = 0f,
					m11 = 1f,
					m12 = 0f,
					m13 = @float.y,
					m20 = 0f,
					m21 = 0f,
					m22 = 1f,
					m23 = @float.z,
					m30 = 0f,
					m31 = 0f,
					m32 = 0f,
					m33 = 1f
				}.m00 == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "Quat Fast vs Quat", delegate(int iter)
		{
			for (int i = 0; i < 4 * iter; i++)
			{
				if (Matrix4x4.TRS(new float3(0.2f, 0.2f, 0.2f), FastMatrix.RotateYAngle(90f), new float3(0f, 0f, 0f)).m00 == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < 4 * iter; i++)
			{
				if (Matrix4x4.TRS(new float3(0.2f, 0.2f, 0.2f), Quaternion.Euler(0f, 90f, 0f), new float3(0f, 0f, 0f)).m00 == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "float4 vs Vector4 Unity.Mathematics", delegate(int iter)
		{
			for (int i = 0; i < 5 * iter; i++)
			{
				float4 x = new float4(1f, 2f, 3f, 4f);
				x.x += 5f;
				float4 @float = new float4(1f, 2f, 3f, 5f);
				x += @float * 5f;
				x -= @float * 2f;
				if (x.Equals(@float))
				{
					_flag++;
				}
				if (math.saturate(x).x > 0f)
				{
					_flag++;
				}
				if (math.abs(x).x == 123f)
				{
					_flag++;
				}
				if (math.length(x) == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < 5 * iter; i++)
			{
				Vector4 vector = new Vector4(1f, 2f, 3f, 4f);
				vector.x += 5f;
				Vector4 vector2 = new Vector4(1f, 2f, 3f, 5f);
				vector += vector2 * 5f;
				vector -= vector2 * 2f;
				if (vector.Equals(vector2))
				{
					_flag++;
				}
				if (math.saturate(vector).x > 0f)
				{
					_flag++;
				}
				if (math.abs(vector).x == 123f)
				{
					_flag++;
				}
				if (math.length(vector) == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "float4 vs Vector4 Mathf", delegate(int iter)
		{
			for (int i = 0; i < 5 * iter; i++)
			{
				float4 x = new float4(1f, 2f, 3f, 4f);
				x.x += 5f;
				float4 @float = new float4(1f, 2f, 3f, 5f);
				x += @float * 5f;
				x -= @float * 2f;
				if (x.Equals(@float))
				{
					_flag++;
				}
				if (math.length(math.saturate(x)) > 0f)
				{
					_flag++;
				}
				if (math.length(math.abs(x)) == 123f)
				{
					_flag++;
				}
				if (math.length(x) == 123f)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < 5 * iter; i++)
			{
				Vector4 vector = new Vector4(1f, 2f, 3f, 4f);
				vector.x += 5f;
				Vector4 vector2 = new Vector4(1f, 2f, 3f, 5f);
				vector += vector2 * 5f;
				vector -= vector2 * 2f;
				if (vector.Equals(vector2))
				{
					_flag++;
				}
				if (new Vector4(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y), Mathf.Clamp01(vector.z), Mathf.Clamp01(vector.w)).magnitude > 0f)
				{
					_flag++;
				}
				if (new Vector4(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z), Mathf.Abs(vector.w)).magnitude == 123f)
				{
					_flag++;
				}
				if (vector.magnitude == 123f)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "math vs Mathf", delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				if (math.abs(i) == 123)
				{
					_flag++;
				}
				if (math.ceil(i) == 123f)
				{
					_flag++;
				}
				if (math.pow(i, 2f) == 123f)
				{
					_flag++;
				}
				if (math.clamp(i, 0, 1000) == 123)
				{
					_flag++;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < 10 * iter; i++)
			{
				if (Mathf.Abs(i) == 123)
				{
					_flag++;
				}
				if (Mathf.Ceil(i) == 123f)
				{
					_flag++;
				}
				if (Mathf.Pow(i, 2f) == 123f)
				{
					_flag++;
				}
				if (Mathf.Clamp(i, 0, 1000) == 123)
				{
					_flag++;
				}
			}
		});
		Bench(ctx, "Matrix4x4 NoRef vs Ref", delegate(int iter)
		{
			Matrix4x4 test = Matrix4x4.Translate(new Vector3(1f, 2f, 3f));
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += Matrix4_NoRef(test);
			}
		}, delegate(int iter)
		{
			Matrix4x4 test = Matrix4x4.Translate(new Vector3(1f, 2f, 3f));
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += Matrix4_Ref(ref test);
			}
		});
		Bench(ctx, "Matrix4x4 Ref vs In", delegate(int iter)
		{
			Matrix4x4 test = Matrix4x4.Translate(new Vector3(1f, 2f, 3f));
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += Matrix4_Ref(ref test);
			}
		}, delegate(int iter)
		{
			Matrix4x4 test = Matrix4x4.Translate(new Vector3(1f, 2f, 3f));
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += Matrix4_In(in test);
			}
		});
		Bench(ctx, "float4x4 NoRef vs Ref", delegate(int iter)
		{
			float4x4 test = Matrix4x4.Translate(new Vector3(1f, 2f, 3f));
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += float4_NoRef(test);
			}
		}, delegate(int iter)
		{
			float4x4 test = Matrix4x4.Translate(new Vector3(1f, 2f, 3f));
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += float4_Ref(ref test);
			}
		});
		Bench(ctx, "float4x4 Conversion vs No Conversion", delegate(int iter)
		{
			float4x4 float4x = Matrix4x4.Translate(new Vector3(1f, 2f, 3f));
			for (int i = 0; i < 10 * iter; i++)
			{
				_flag += Matrix4_NoRef(float4x);
			}
		}, delegate(int iter)
		{
			float4x4 test = Matrix4x4.Translate(new Vector3(1f, 2f, 3f));
			for (int i = 0; i < 10 * iter; i++)
			{
				_flag += float4_NoRef(test);
			}
		});
		Bench(ctx, "float4 NoRef vs Ref", delegate(int iter)
		{
			float4 test = new float4(1f, 2f, 3f, 4f);
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += float_v_NoRef(test);
			}
		}, delegate(int iter)
		{
			float4 test = new float4(1f, 2f, 3f, 4f);
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += float_v_Ref(ref test);
			}
		});
		Bench(ctx, "float4 Ref vs In", delegate(int iter)
		{
			float4 test = new float4(1f, 2f, 3f, 4f);
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += float_v_Ref(ref test);
			}
		}, delegate(int iter)
		{
			float4 test = new float4(1f, 2f, 3f, 4f);
			for (int i = 0; i < 100 * iter; i++)
			{
				_flag += float_v_In(in test);
			}
		});
		Bench(ctx, "mesh==null vs null==mesh", delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if (mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		}, delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if (null == mesh)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		});
		Bench(ctx, "mesh==null vs ReferenceEquals", delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if (mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		}, delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if ((object)mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		});
		Bench(ctx, "mesh==null vs (obj)mesh==null", delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if (mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		}, delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if ((object)mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		});
		Bench(ctx, "mesh==null vs mesh is null", delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if (mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		}, delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if ((object)mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		});
		Bench(ctx, "mesh==null vs mesh is not Mesh", delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if (mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		}, delegate(int iter)
		{
			Mesh mesh = new Mesh();
			for (int i = 0; i < 100 * iter; i++)
			{
				if ((object)mesh == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		});
		Bench(ctx, "item==null vs item is null", delegate(int iter)
		{
			ShapeItem shapeItem = new ShapeItem(null);
			for (int i = 0; i < 100 * iter; i++)
			{
				if (shapeItem == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		}, delegate(int iter)
		{
			ShapeItem shapeItem = new ShapeItem(null);
			for (int i = 0; i < 100 * iter; i++)
			{
				if (shapeItem == null)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		});
		Bench(ctx, "list vs array - access", delegate(int iter)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < 50; i++)
			{
				list.Add(i);
			}
			for (int j = 0; j < 5 * iter; j++)
			{
				for (int k = 0; k < list.Count; k++)
				{
					if (list[k] == j)
					{
						int num = _flag + 1;
						_flag = num;
					}
				}
			}
		}, delegate(int iter)
		{
			int[] array = new int[50];
			for (int i = 0; i < 50; i++)
			{
				array[i] = i;
			}
			for (int j = 0; j < 5 * iter; j++)
			{
				for (int k = 0; k < array.Length; k++)
				{
					if (array[k] == j)
					{
						int num = _flag + 1;
						_flag = num;
					}
				}
			}
		});
		Bench(ctx, "list vs array - resize", delegate(int iter)
		{
			for (int i = 0; i < iter / 10; i++)
			{
				List<int> list = new List<int>();
				for (int j = 0; j < 50; j++)
				{
					list.Add(j);
				}
				if (list[23] == 59)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		}, delegate(int iter)
		{
			for (int i = 0; i < iter / 10; i++)
			{
				int[] array = new int[0];
				for (int j = 0; j < 50; j++)
				{
					Array.Resize(ref array, array.Length + 1);
					array[^1] = j;
				}
				if (array[23] == 59)
				{
					int num = _flag + 1;
					_flag = num;
				}
			}
		});
	}

	public static int Matrix4_NoRef(Matrix4x4 test1)
	{
		float i = 0f;
		i += test1.m00;
		i += test1.m10;
		i += test1.m20;
		i += test1.m30;
		i += test1.m01;
		i += test1.m11;
		i += test1.m21;
		i += test1.m31;
		i += test1.m02;
		i += test1.m12;
		i += test1.m22;
		i += test1.m32;
		i += test1.m03;
		i += test1.m13;
		i += test1.m23;
		i += test1.m33;
		return (i > 5f) ? 1 : 0;
	}

	public static int Matrix4_Ref(ref Matrix4x4 test1)
	{
		float i = 0f;
		i += test1.m00;
		i += test1.m10;
		i += test1.m20;
		i += test1.m30;
		i += test1.m01;
		i += test1.m11;
		i += test1.m21;
		i += test1.m31;
		i += test1.m02;
		i += test1.m12;
		i += test1.m22;
		i += test1.m32;
		i += test1.m03;
		i += test1.m13;
		i += test1.m23;
		i += test1.m33;
		return (i > 5f) ? 1 : 0;
	}

	public static int Matrix4_In(in Matrix4x4 test1)
	{
		float i = 0f;
		i += test1.m00;
		i += test1.m10;
		i += test1.m20;
		i += test1.m30;
		i += test1.m01;
		i += test1.m11;
		i += test1.m21;
		i += test1.m31;
		i += test1.m02;
		i += test1.m12;
		i += test1.m22;
		i += test1.m32;
		i += test1.m03;
		i += test1.m13;
		i += test1.m23;
		i += test1.m33;
		return (i > 5f) ? 1 : 0;
	}

	public static int float4_NoRef(float4x4 test1)
	{
		float i = 0f;
		i += test1.c0.x;
		i += test1.c0.y;
		i += test1.c0.z;
		i += test1.c0.w;
		i += test1.c1.x;
		i += test1.c1.y;
		i += test1.c1.z;
		i += test1.c1.w;
		i += test1.c2.x;
		i += test1.c2.y;
		i += test1.c2.z;
		i += test1.c2.w;
		i += test1.c3.x;
		i += test1.c3.y;
		i += test1.c3.z;
		i += test1.c3.w;
		return (i > 5f) ? 1 : 0;
	}

	public static int float4_Ref(ref float4x4 test1)
	{
		float i = 0f;
		i += test1.c0.x;
		i += test1.c0.y;
		i += test1.c0.z;
		i += test1.c0.w;
		i += test1.c1.x;
		i += test1.c1.y;
		i += test1.c1.z;
		i += test1.c1.w;
		i += test1.c2.x;
		i += test1.c2.y;
		i += test1.c2.z;
		i += test1.c2.w;
		i += test1.c3.x;
		i += test1.c3.y;
		i += test1.c3.z;
		i += test1.c3.w;
		return (i > 5f) ? 1 : 0;
	}

	public static int float_v_Ref(ref float4 test1)
	{
		float i = 0f;
		i += test1.x;
		i += test1.y;
		i += test1.z;
		i += test1.w;
		return (i > 5f) ? 1 : 0;
	}

	public static int float_v_NoRef(float4 test1)
	{
		float i = 0f;
		i += test1.x;
		i += test1.y;
		i += test1.z;
		i += test1.w;
		return (i > 5f) ? 1 : 0;
	}

	public static int float_v_In(in float4 test1)
	{
		float i = 0f;
		i += test1.x;
		i += test1.y;
		i += test1.z;
		i += test1.w;
		return (i > 5f) ? 1 : 0;
	}
}
