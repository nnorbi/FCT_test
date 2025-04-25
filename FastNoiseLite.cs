using System;
using System.Runtime.CompilerServices;

public struct FastNoiseLite
{
	private enum TransformType3D
	{
		None,
		ImproveXYPlanes,
		ImproveXZPlanes,
		DefaultOpenSimplex2
	}

	public enum NoiseType
	{
		OpenSimplex2,
		OpenSimplex2S,
		Cellular,
		Perlin,
		ValueCubic,
		Value
	}

	public enum RotationType3D
	{
		None,
		ImproveXYPlanes,
		ImproveXZPlanes
	}

	public enum FractalType
	{
		None,
		FBm,
		Ridged,
		PingPong,
		DomainWarpProgressive,
		DomainWarpIndependent
	}

	public enum CellularDistanceFunction
	{
		Euclidean,
		EuclideanSq,
		Manhattan,
		Hybrid
	}

	public enum CellularReturnType
	{
		CellValue,
		Distance,
		Distance2,
		Distance2Add,
		Distance2Sub,
		Distance2Mul,
		Distance2Div
	}

	public enum DomainWarpType
	{
		OpenSimplex2,
		OpenSimplex2Reduced,
		BasicGrid
	}

	private const short INLINE = 256;

	private const short OPTIMISE = 512;

	private const int PrimeX = 501125321;

	private const int PrimeY = 1136930381;

	private const int PrimeZ = 1720413743;

	private static readonly float[] Gradients2D = new float[256]
	{
		0.13052619f, 0.9914449f, 0.38268343f, 0.9238795f, 0.6087614f, 0.7933533f, 0.7933533f, 0.6087614f, 0.9238795f, 0.38268343f,
		0.9914449f, 0.13052619f, 0.9914449f, -0.13052619f, 0.9238795f, -0.38268343f, 0.7933533f, -0.6087614f, 0.6087614f, -0.7933533f,
		0.38268343f, -0.9238795f, 0.13052619f, -0.9914449f, -0.13052619f, -0.9914449f, -0.38268343f, -0.9238795f, -0.6087614f, -0.7933533f,
		-0.7933533f, -0.6087614f, -0.9238795f, -0.38268343f, -0.9914449f, -0.13052619f, -0.9914449f, 0.13052619f, -0.9238795f, 0.38268343f,
		-0.7933533f, 0.6087614f, -0.6087614f, 0.7933533f, -0.38268343f, 0.9238795f, -0.13052619f, 0.9914449f, 0.13052619f, 0.9914449f,
		0.38268343f, 0.9238795f, 0.6087614f, 0.7933533f, 0.7933533f, 0.6087614f, 0.9238795f, 0.38268343f, 0.9914449f, 0.13052619f,
		0.9914449f, -0.13052619f, 0.9238795f, -0.38268343f, 0.7933533f, -0.6087614f, 0.6087614f, -0.7933533f, 0.38268343f, -0.9238795f,
		0.13052619f, -0.9914449f, -0.13052619f, -0.9914449f, -0.38268343f, -0.9238795f, -0.6087614f, -0.7933533f, -0.7933533f, -0.6087614f,
		-0.9238795f, -0.38268343f, -0.9914449f, -0.13052619f, -0.9914449f, 0.13052619f, -0.9238795f, 0.38268343f, -0.7933533f, 0.6087614f,
		-0.6087614f, 0.7933533f, -0.38268343f, 0.9238795f, -0.13052619f, 0.9914449f, 0.13052619f, 0.9914449f, 0.38268343f, 0.9238795f,
		0.6087614f, 0.7933533f, 0.7933533f, 0.6087614f, 0.9238795f, 0.38268343f, 0.9914449f, 0.13052619f, 0.9914449f, -0.13052619f,
		0.9238795f, -0.38268343f, 0.7933533f, -0.6087614f, 0.6087614f, -0.7933533f, 0.38268343f, -0.9238795f, 0.13052619f, -0.9914449f,
		-0.13052619f, -0.9914449f, -0.38268343f, -0.9238795f, -0.6087614f, -0.7933533f, -0.7933533f, -0.6087614f, -0.9238795f, -0.38268343f,
		-0.9914449f, -0.13052619f, -0.9914449f, 0.13052619f, -0.9238795f, 0.38268343f, -0.7933533f, 0.6087614f, -0.6087614f, 0.7933533f,
		-0.38268343f, 0.9238795f, -0.13052619f, 0.9914449f, 0.13052619f, 0.9914449f, 0.38268343f, 0.9238795f, 0.6087614f, 0.7933533f,
		0.7933533f, 0.6087614f, 0.9238795f, 0.38268343f, 0.9914449f, 0.13052619f, 0.9914449f, -0.13052619f, 0.9238795f, -0.38268343f,
		0.7933533f, -0.6087614f, 0.6087614f, -0.7933533f, 0.38268343f, -0.9238795f, 0.13052619f, -0.9914449f, -0.13052619f, -0.9914449f,
		-0.38268343f, -0.9238795f, -0.6087614f, -0.7933533f, -0.7933533f, -0.6087614f, -0.9238795f, -0.38268343f, -0.9914449f, -0.13052619f,
		-0.9914449f, 0.13052619f, -0.9238795f, 0.38268343f, -0.7933533f, 0.6087614f, -0.6087614f, 0.7933533f, -0.38268343f, 0.9238795f,
		-0.13052619f, 0.9914449f, 0.13052619f, 0.9914449f, 0.38268343f, 0.9238795f, 0.6087614f, 0.7933533f, 0.7933533f, 0.6087614f,
		0.9238795f, 0.38268343f, 0.9914449f, 0.13052619f, 0.9914449f, -0.13052619f, 0.9238795f, -0.38268343f, 0.7933533f, -0.6087614f,
		0.6087614f, -0.7933533f, 0.38268343f, -0.9238795f, 0.13052619f, -0.9914449f, -0.13052619f, -0.9914449f, -0.38268343f, -0.9238795f,
		-0.6087614f, -0.7933533f, -0.7933533f, -0.6087614f, -0.9238795f, -0.38268343f, -0.9914449f, -0.13052619f, -0.9914449f, 0.13052619f,
		-0.9238795f, 0.38268343f, -0.7933533f, 0.6087614f, -0.6087614f, 0.7933533f, -0.38268343f, 0.9238795f, -0.13052619f, 0.9914449f,
		0.38268343f, 0.9238795f, 0.9238795f, 0.38268343f, 0.9238795f, -0.38268343f, 0.38268343f, -0.9238795f, -0.38268343f, -0.9238795f,
		-0.9238795f, -0.38268343f, -0.9238795f, 0.38268343f, -0.38268343f, 0.9238795f
	};

	private static readonly float[] RandVecs2D = new float[512]
	{
		-0.2700222f, -0.9628541f, 0.38630927f, -0.9223693f, 0.04444859f, -0.9990117f, -0.59925234f, -0.80056024f, -0.781928f, 0.62336874f,
		0.9464672f, 0.32279992f, -0.6514147f, -0.7587219f, 0.93784726f, 0.34704837f, -0.8497876f, -0.52712524f, -0.87904257f, 0.47674325f,
		-0.8923003f, -0.45144236f, -0.37984443f, -0.9250504f, -0.9951651f, 0.09821638f, 0.7724398f, -0.635088f, 0.75732833f, -0.6530343f,
		-0.9928005f, -0.119780056f, -0.05326657f, 0.99858034f, 0.97542536f, -0.22033007f, -0.76650184f, 0.64224213f, 0.9916367f, 0.12906061f,
		-0.99469686f, 0.10285038f, -0.53792053f, -0.8429955f, 0.50228155f, -0.86470413f, 0.45598215f, -0.8899889f, -0.8659131f, -0.50019443f,
		0.08794584f, -0.9961253f, -0.5051685f, 0.8630207f, 0.7753185f, -0.6315704f, -0.69219446f, 0.72171104f, -0.51916593f, -0.85467345f,
		0.8978623f, -0.4402764f, -0.17067741f, 0.98532695f, -0.935343f, -0.35374206f, -0.99924046f, 0.038967468f, -0.2882064f, -0.9575683f,
		-0.96638113f, 0.2571138f, -0.87597144f, -0.48236302f, -0.8303123f, -0.55729836f, 0.051101338f, -0.99869347f, -0.85583735f, -0.51724505f,
		0.098870255f, 0.9951003f, 0.9189016f, 0.39448678f, -0.24393758f, -0.96979094f, -0.81214094f, -0.5834613f, -0.99104315f, 0.13354214f,
		0.8492424f, -0.52800316f, -0.9717839f, -0.23587295f, 0.9949457f, 0.10041421f, 0.6241065f, -0.7813392f, 0.6629103f, 0.74869883f,
		-0.7197418f, 0.6942418f, -0.8143371f, -0.58039224f, 0.10452105f, -0.9945227f, -0.10659261f, -0.99430275f, 0.44579968f, -0.8951328f,
		0.105547406f, 0.99441427f, -0.9927903f, 0.11986445f, -0.83343667f, 0.55261505f, 0.9115562f, -0.4111756f, 0.8285545f, -0.55990845f,
		0.7217098f, -0.6921958f, 0.49404928f, -0.8694339f, -0.36523214f, -0.9309165f, -0.9696607f, 0.24445485f, 0.089255095f, -0.9960088f,
		0.5354071f, -0.8445941f, -0.10535762f, 0.9944344f, -0.98902845f, 0.1477251f, 0.004856105f, 0.9999882f, 0.98855984f, 0.15082914f,
		0.92861295f, -0.37104982f, -0.5832394f, -0.8123003f, 0.30152076f, 0.9534596f, -0.95751107f, 0.28839657f, 0.9715802f, -0.23671055f,
		0.2299818f, 0.97319496f, 0.9557638f, -0.2941352f, 0.7409561f, 0.67155343f, -0.9971514f, -0.07542631f, 0.69057107f, -0.7232645f,
		-0.2907137f, -0.9568101f, 0.5912778f, -0.80646795f, -0.94545925f, -0.3257405f, 0.66644555f, 0.7455537f, 0.6236135f, 0.78173286f,
		0.9126994f, -0.40863165f, -0.8191762f, 0.57354194f, -0.8812746f, -0.4726046f, 0.99533135f, 0.09651673f, 0.98556507f, -0.16929697f,
		-0.8495981f, 0.52743065f, 0.6174854f, -0.78658235f, 0.85081565f, 0.5254643f, 0.99850327f, -0.0546925f, 0.19713716f, -0.98037595f,
		0.66078556f, -0.7505747f, -0.030974941f, 0.9995202f, -0.6731661f, 0.73949134f, -0.71950185f, -0.69449055f, 0.97275114f, 0.2318516f,
		0.9997059f, -0.02425069f, 0.44217876f, -0.89692694f, 0.9981351f, -0.061043672f, -0.9173661f, -0.39804456f, -0.81500566f, -0.579453f,
		-0.87893313f, 0.476945f, 0.015860584f, 0.99987423f, -0.8095465f, 0.5870558f, -0.9165899f, -0.39982867f, -0.8023543f, 0.5968481f,
		-0.5176738f, 0.85557806f, -0.8154407f, -0.57884055f, 0.40220103f, -0.91555136f, -0.9052557f, -0.4248672f, 0.7317446f, 0.681579f,
		-0.56476325f, -0.825253f, -0.8403276f, -0.54207885f, -0.93142813f, 0.36392525f, 0.52381986f, 0.85182905f, 0.7432804f, -0.66898f,
		-0.9853716f, -0.17041974f, 0.46014687f, 0.88784283f, 0.8258554f, 0.56388193f, 0.6182366f, 0.785992f, 0.83315027f, -0.55304664f,
		0.15003075f, 0.9886813f, -0.6623304f, -0.7492119f, -0.66859865f, 0.74362344f, 0.7025606f, 0.7116239f, -0.54193896f, -0.84041786f,
		-0.33886164f, 0.9408362f, 0.833153f, 0.55304253f, -0.29897207f, -0.95426184f, 0.2638523f, 0.9645631f, 0.12410874f, -0.9922686f,
		-0.7282649f, -0.6852957f, 0.69625f, 0.71779937f, -0.91835356f, 0.395761f, -0.6326102f, -0.7744703f, -0.9331892f, -0.35938552f,
		-0.11537793f, -0.99332166f, 0.9514975f, -0.30765656f, -0.08987977f, -0.9959526f, 0.6678497f, 0.7442962f, 0.79524004f, -0.6062947f,
		-0.6462007f, -0.7631675f, -0.27335986f, 0.96191186f, 0.966959f, -0.25493184f, -0.9792895f, 0.20246519f, -0.5369503f, -0.84361386f,
		-0.27003646f, -0.9628501f, -0.6400277f, 0.76835185f, -0.78545374f, -0.6189204f, 0.060059056f, -0.9981948f, -0.024557704f, 0.9996984f,
		-0.65983623f, 0.7514095f, -0.62538946f, -0.7803128f, -0.6210409f, -0.7837782f, 0.8348889f, 0.55041856f, -0.15922752f, 0.9872419f,
		0.83676225f, 0.54756635f, -0.8675754f, -0.4973057f, -0.20226626f, -0.97933054f, 0.939919f, 0.34139755f, 0.98774046f, -0.1561049f,
		-0.90344554f, 0.42870283f, 0.12698042f, -0.9919052f, -0.3819601f, 0.92417884f, 0.9754626f, 0.22016525f, -0.32040158f, -0.94728184f,
		-0.9874761f, 0.15776874f, 0.025353484f, -0.99967855f, 0.4835131f, -0.8753371f, -0.28508f, -0.9585037f, -0.06805516f, -0.99768156f,
		-0.7885244f, -0.61500347f, 0.3185392f, -0.9479097f, 0.8880043f, 0.45983514f, 0.64769214f, -0.76190215f, 0.98202413f, 0.18875542f,
		0.93572754f, -0.35272372f, -0.88948953f, 0.45695552f, 0.7922791f, 0.6101588f, 0.74838185f, 0.66326815f, -0.728893f, -0.68462765f,
		0.8729033f, -0.48789328f, 0.8288346f, 0.5594937f, 0.08074567f, 0.99673474f, 0.97991484f, -0.1994165f, -0.5807307f, -0.81409574f,
		-0.47000498f, -0.8826638f, 0.2409493f, 0.9705377f, 0.9437817f, -0.33056942f, -0.89279985f, -0.45045355f, -0.80696225f, 0.59060305f,
		0.062589735f, 0.99803936f, -0.93125975f, 0.36435598f, 0.57774496f, 0.81621736f, -0.3360096f, -0.9418586f, 0.69793206f, -0.71616393f,
		-0.0020081573f, -0.999998f, -0.18272944f, -0.98316324f, -0.6523912f, 0.7578824f, -0.43026268f, -0.9027037f, -0.9985126f, -0.054520912f,
		-0.010281022f, -0.99994713f, -0.49460712f, 0.86911666f, -0.299935f, 0.95395964f, 0.8165472f, 0.5772787f, 0.26974604f, 0.9629315f,
		-0.7306287f, -0.68277496f, -0.7590952f, -0.65097964f, -0.9070538f, 0.4210146f, -0.5104861f, -0.859886f, 0.86133504f, 0.5080373f,
		0.50078815f, -0.8655699f, -0.6541582f, 0.7563578f, -0.83827555f, -0.54524684f, 0.6940071f, 0.7199682f, 0.06950936f, 0.9975813f,
		0.17029423f, -0.9853933f, 0.26959732f, 0.9629731f, 0.55196124f, -0.83386976f, 0.2256575f, -0.9742067f, 0.42152628f, -0.9068162f,
		0.48818734f, -0.87273884f, -0.3683855f, -0.92967314f, -0.98253906f, 0.18605645f, 0.81256473f, 0.582871f, 0.3196461f, -0.947537f,
		0.9570914f, 0.28978625f, -0.6876655f, -0.7260276f, -0.9988771f, -0.04737673f, -0.1250179f, 0.9921545f, -0.82801336f, 0.56070834f,
		0.93248636f, -0.36120513f, 0.63946533f, 0.7688199f, -0.016238471f, -0.99986815f, -0.99550146f, -0.094746135f, -0.8145332f, 0.580117f,
		0.4037328f, -0.91487694f, 0.9944263f, 0.10543368f, -0.16247116f, 0.9867133f, -0.9949488f, -0.10038388f, -0.69953024f, 0.714603f,
		0.5263415f, -0.85027325f, -0.5395222f, 0.8419714f, 0.65793705f, 0.7530729f, 0.014267588f, -0.9998982f, -0.6734384f, 0.7392433f,
		0.6394121f, -0.7688642f, 0.9211571f, 0.38919085f, -0.14663722f, -0.98919034f, -0.7823181f, 0.6228791f, -0.5039611f, -0.8637264f,
		-0.774312f, -0.632804f
	};

	private static readonly float[] Gradients3D = new float[256]
	{
		0f, 1f, 1f, 0f, 0f, -1f, 1f, 0f, 0f, 1f,
		-1f, 0f, 0f, -1f, -1f, 0f, 1f, 0f, 1f, 0f,
		-1f, 0f, 1f, 0f, 1f, 0f, -1f, 0f, -1f, 0f,
		-1f, 0f, 1f, 1f, 0f, 0f, -1f, 1f, 0f, 0f,
		1f, -1f, 0f, 0f, -1f, -1f, 0f, 0f, 0f, 1f,
		1f, 0f, 0f, -1f, 1f, 0f, 0f, 1f, -1f, 0f,
		0f, -1f, -1f, 0f, 1f, 0f, 1f, 0f, -1f, 0f,
		1f, 0f, 1f, 0f, -1f, 0f, -1f, 0f, -1f, 0f,
		1f, 1f, 0f, 0f, -1f, 1f, 0f, 0f, 1f, -1f,
		0f, 0f, -1f, -1f, 0f, 0f, 0f, 1f, 1f, 0f,
		0f, -1f, 1f, 0f, 0f, 1f, -1f, 0f, 0f, -1f,
		-1f, 0f, 1f, 0f, 1f, 0f, -1f, 0f, 1f, 0f,
		1f, 0f, -1f, 0f, -1f, 0f, -1f, 0f, 1f, 1f,
		0f, 0f, -1f, 1f, 0f, 0f, 1f, -1f, 0f, 0f,
		-1f, -1f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, -1f,
		1f, 0f, 0f, 1f, -1f, 0f, 0f, -1f, -1f, 0f,
		1f, 0f, 1f, 0f, -1f, 0f, 1f, 0f, 1f, 0f,
		-1f, 0f, -1f, 0f, -1f, 0f, 1f, 1f, 0f, 0f,
		-1f, 1f, 0f, 0f, 1f, -1f, 0f, 0f, -1f, -1f,
		0f, 0f, 0f, 1f, 1f, 0f, 0f, -1f, 1f, 0f,
		0f, 1f, -1f, 0f, 0f, -1f, -1f, 0f, 1f, 0f,
		1f, 0f, -1f, 0f, 1f, 0f, 1f, 0f, -1f, 0f,
		-1f, 0f, -1f, 0f, 1f, 1f, 0f, 0f, -1f, 1f,
		0f, 0f, 1f, -1f, 0f, 0f, -1f, -1f, 0f, 0f,
		1f, 1f, 0f, 0f, 0f, -1f, 1f, 0f, -1f, 1f,
		0f, 0f, 0f, -1f, -1f, 0f
	};

	private static readonly float[] RandVecs3D = new float[1024]
	{
		-0.7292737f, -0.66184396f, 0.17355819f, 0f, 0.7902921f, -0.5480887f, -0.2739291f, 0f, 0.7217579f, 0.62262124f,
		-0.3023381f, 0f, 0.5656831f, -0.8208298f, -0.079000026f, 0f, 0.76004905f, -0.55559796f, -0.33709997f, 0f,
		0.37139457f, 0.50112647f, 0.78162545f, 0f, -0.12770624f, -0.4254439f, -0.8959289f, 0f, -0.2881561f, -0.5815839f,
		0.7607406f, 0f, 0.5849561f, -0.6628202f, -0.4674352f, 0f, 0.33071712f, 0.039165374f, 0.94291687f, 0f,
		0.8712122f, -0.41133744f, -0.26793817f, 0f, 0.580981f, 0.7021916f, 0.41156778f, 0f, 0.5037569f, 0.6330057f,
		-0.5878204f, 0f, 0.44937122f, 0.6013902f, 0.6606023f, 0f, -0.6878404f, 0.090188906f, -0.7202372f, 0f,
		-0.59589565f, -0.64693505f, 0.47579765f, 0f, -0.5127052f, 0.1946922f, -0.83619875f, 0f, -0.99115074f, -0.054102764f,
		-0.12121531f, 0f, -0.21497211f, 0.9720882f, -0.09397608f, 0f, -0.7518651f, -0.54280573f, 0.37424695f, 0f,
		0.5237069f, 0.8516377f, -0.021078179f, 0f, 0.6333505f, 0.19261672f, -0.74951047f, 0f, -0.06788242f, 0.39983058f,
		0.9140719f, 0f, -0.55386287f, -0.47298968f, -0.6852129f, 0f, -0.72614557f, -0.5911991f, 0.35099334f, 0f,
		-0.9229275f, -0.17828088f, 0.34120494f, 0f, -0.6968815f, 0.65112746f, 0.30064803f, 0f, 0.96080446f, -0.20983632f,
		-0.18117249f, 0f, 0.068171464f, -0.9743405f, 0.21450691f, 0f, -0.3577285f, -0.6697087f, -0.65078455f, 0f,
		-0.18686211f, 0.7648617f, -0.61649746f, 0f, -0.65416974f, 0.3967915f, 0.64390874f, 0f, 0.699334f, -0.6164538f,
		0.36182392f, 0f, -0.15466657f, 0.6291284f, 0.7617583f, 0f, -0.6841613f, -0.2580482f, -0.68215424f, 0f,
		0.5383981f, 0.4258655f, 0.727163f, 0f, -0.5026988f, -0.7939833f, -0.3418837f, 0f, 0.32029718f, 0.28344154f,
		0.9039196f, 0f, 0.86832273f, -0.00037626564f, -0.49599952f, 0f, 0.79112005f, -0.085110456f, 0.60571057f, 0f,
		-0.04011016f, -0.43972486f, 0.8972364f, 0f, 0.914512f, 0.35793462f, -0.18854876f, 0f, -0.96120393f, -0.27564842f,
		0.010246669f, 0f, 0.65103614f, -0.28777993f, -0.70237786f, 0f, -0.20417863f, 0.73652375f, 0.6448596f, 0f,
		-0.7718264f, 0.37906268f, 0.5104856f, 0f, -0.30600828f, -0.7692988f, 0.56083715f, 0f, 0.45400733f, -0.5024843f,
		0.73578995f, 0f, 0.48167956f, 0.6021208f, -0.636738f, 0f, 0.69619805f, -0.32221973f, 0.6414692f, 0f,
		-0.65321606f, -0.6781149f, 0.33685157f, 0f, 0.50893015f, -0.61546624f, -0.60182345f, 0f, -0.16359198f, -0.9133605f,
		-0.37284088f, 0f, 0.5240802f, -0.8437664f, 0.11575059f, 0f, 0.5902587f, 0.4983818f, -0.63498837f, 0f,
		0.5863228f, 0.49476475f, 0.6414308f, 0f, 0.6779335f, 0.23413453f, 0.6968409f, 0f, 0.7177054f, -0.68589795f,
		0.12017863f, 0f, -0.532882f, -0.5205125f, 0.6671608f, 0f, -0.8654874f, -0.07007271f, -0.4960054f, 0f,
		-0.286181f, 0.79520893f, 0.53454953f, 0f, -0.048495296f, 0.98108363f, -0.18741156f, 0f, -0.63585216f, 0.60583484f,
		0.47818002f, 0f, 0.62547946f, -0.28616196f, 0.72586966f, 0f, -0.258526f, 0.50619495f, -0.8227582f, 0f,
		0.021363068f, 0.50640166f, -0.862033f, 0f, 0.20011178f, 0.85992634f, 0.46955505f, 0f, 0.47435614f, 0.6014985f,
		-0.6427953f, 0f, 0.6622994f, -0.52024746f, -0.539168f, 0f, 0.08084973f, -0.65327203f, 0.7527941f, 0f,
		-0.6893687f, 0.059286036f, 0.7219805f, 0f, -0.11218871f, -0.96731853f, 0.22739525f, 0f, 0.7344116f, 0.59796685f,
		-0.3210533f, 0f, 0.5789393f, -0.24888498f, 0.776457f, 0f, 0.69881827f, 0.35571697f, -0.6205791f, 0f,
		-0.86368454f, -0.27487713f, -0.4224826f, 0f, -0.4247028f, -0.46408808f, 0.77733505f, 0f, 0.5257723f, -0.84270173f,
		0.11583299f, 0f, 0.93438303f, 0.31630248f, -0.16395439f, 0f, -0.10168364f, -0.8057303f, -0.58348876f, 0f,
		-0.6529239f, 0.50602126f, -0.5635893f, 0f, -0.24652861f, -0.9668206f, -0.06694497f, 0f, -0.9776897f, -0.20992506f,
		-0.0073688254f, 0f, 0.7736893f, 0.57342446f, 0.2694238f, 0f, -0.6095088f, 0.4995679f, 0.6155737f, 0f,
		0.5794535f, 0.7434547f, 0.33392924f, 0f, -0.8226211f, 0.081425816f, 0.56272936f, 0f, -0.51038545f, 0.47036678f,
		0.719904f, 0f, -0.5764972f, -0.072316565f, -0.81389266f, 0f, 0.7250629f, 0.39499715f, -0.56414634f, 0f,
		-0.1525424f, 0.48608407f, -0.8604958f, 0f, -0.55509764f, -0.49578208f, 0.6678823f, 0f, -0.18836144f, 0.91458696f,
		0.35784173f, 0f, 0.76255566f, -0.54144084f, -0.35404897f, 0f, -0.5870232f, -0.3226498f, -0.7424964f, 0f,
		0.30511242f, 0.2262544f, -0.9250488f, 0f, 0.63795763f, 0.57724243f, -0.50970703f, 0f, -0.5966776f, 0.14548524f,
		-0.7891831f, 0f, -0.65833056f, 0.65554875f, -0.36994147f, 0f, 0.74348927f, 0.23510846f, 0.6260573f, 0f,
		0.5562114f, 0.82643604f, -0.08736329f, 0f, -0.302894f, -0.8251527f, 0.47684193f, 0f, 0.11293438f, -0.9858884f,
		-0.123571075f, 0f, 0.5937653f, -0.5896814f, 0.5474657f, 0f, 0.6757964f, -0.58357584f, -0.45026484f, 0f,
		0.7242303f, -0.11527198f, 0.67985505f, 0f, -0.9511914f, 0.0753624f, -0.29925808f, 0f, 0.2539471f, -0.18863393f,
		0.9486454f, 0f, 0.5714336f, -0.16794509f, -0.8032796f, 0f, -0.06778235f, 0.39782694f, 0.9149532f, 0f,
		0.6074973f, 0.73306f, -0.30589226f, 0f, -0.54354787f, 0.16758224f, 0.8224791f, 0f, -0.5876678f, -0.3380045f,
		-0.7351187f, 0f, -0.79675627f, 0.040978227f, -0.60290986f, 0f, -0.19963509f, 0.8706295f, 0.4496111f, 0f,
		-0.027876602f, -0.91062325f, -0.4122962f, 0f, -0.7797626f, -0.6257635f, 0.019757755f, 0f, -0.5211233f, 0.74016446f,
		-0.42495546f, 0f, 0.8575425f, 0.4053273f, -0.31675017f, 0f, 0.10452233f, 0.8390196f, -0.53396744f, 0f,
		0.3501823f, 0.9242524f, -0.15208502f, 0f, 0.19878499f, 0.076476134f, 0.9770547f, 0f, 0.78459966f, 0.6066257f,
		-0.12809642f, 0f, 0.09006737f, -0.97509897f, -0.20265691f, 0f, -0.82743436f, -0.54229957f, 0.14582036f, 0f,
		-0.34857976f, -0.41580227f, 0.8400004f, 0f, -0.2471779f, -0.730482f, -0.6366311f, 0f, -0.3700155f, 0.8577948f,
		0.35675845f, 0f, 0.59133947f, -0.54831195f, -0.59133035f, 0f, 0.120487355f, -0.7626472f, -0.6354935f, 0f,
		0.6169593f, 0.03079648f, 0.7863923f, 0f, 0.12581569f, -0.664083f, -0.73699677f, 0f, -0.6477565f, -0.17401473f,
		-0.74170774f, 0f, 0.6217889f, -0.7804431f, -0.06547655f, 0f, 0.6589943f, -0.6096988f, 0.44044736f, 0f,
		-0.26898375f, -0.6732403f, -0.68876356f, 0f, -0.38497752f, 0.56765425f, 0.7277094f, 0f, 0.57544446f, 0.81104714f,
		-0.10519635f, 0f, 0.91415936f, 0.3832948f, 0.13190056f, 0f, -0.10792532f, 0.9245494f, 0.36545935f, 0f,
		0.3779771f, 0.30431488f, 0.87437165f, 0f, -0.21428852f, -0.8259286f, 0.5214617f, 0f, 0.58025444f, 0.41480985f,
		-0.7008834f, 0f, -0.19826609f, 0.85671616f, -0.47615966f, 0f, -0.033815537f, 0.37731808f, -0.9254661f, 0f,
		-0.68679225f, -0.6656598f, 0.29191336f, 0f, 0.7731743f, -0.28757936f, -0.565243f, 0f, -0.09655942f, 0.91937083f,
		-0.3813575f, 0f, 0.27157024f, -0.957791f, -0.09426606f, 0f, 0.24510157f, -0.6917999f, -0.6792188f, 0f,
		0.97770077f, -0.17538553f, 0.115503654f, 0f, -0.522474f, 0.8521607f, 0.029036159f, 0f, -0.77348804f, -0.52612925f,
		0.35341796f, 0f, -0.71344924f, -0.26954725f, 0.6467878f, 0f, 0.16440372f, 0.5105846f, -0.84396374f, 0f,
		0.6494636f, 0.055856112f, 0.7583384f, 0f, -0.4711971f, 0.50172806f, -0.7254256f, 0f, -0.63357645f, -0.23816863f,
		-0.7361091f, 0f, -0.9021533f, -0.2709478f, -0.33571818f, 0f, -0.3793711f, 0.8722581f, 0.3086152f, 0f,
		-0.68555987f, -0.32501432f, 0.6514394f, 0f, 0.29009423f, -0.7799058f, -0.5546101f, 0f, -0.20983194f, 0.8503707f,
		0.48253515f, 0f, -0.45926037f, 0.6598504f, -0.5947077f, 0f, 0.87159455f, 0.09616365f, -0.48070312f, 0f,
		-0.6776666f, 0.71185046f, -0.1844907f, 0f, 0.7044378f, 0.3124276f, 0.637304f, 0f, -0.7052319f, -0.24010932f,
		-0.6670798f, 0f, 0.081921004f, -0.72073364f, -0.68835455f, 0f, -0.6993681f, -0.5875763f, -0.4069869f, 0f,
		-0.12814544f, 0.6419896f, 0.75592864f, 0f, -0.6337388f, -0.67854714f, -0.3714147f, 0f, 0.5565052f, -0.21688876f,
		-0.8020357f, 0f, -0.57915545f, 0.7244372f, -0.3738579f, 0f, 0.11757791f, -0.7096451f, 0.69467926f, 0f,
		-0.613462f, 0.13236311f, 0.7785528f, 0f, 0.69846356f, -0.029805163f, -0.7150247f, 0f, 0.83180827f, -0.3930172f,
		0.39195976f, 0f, 0.14695764f, 0.055416517f, -0.98758924f, 0f, 0.70886856f, -0.2690504f, 0.65201014f, 0f,
		0.27260533f, 0.67369765f, -0.68688995f, 0f, -0.65912956f, 0.30354586f, -0.68804663f, 0f, 0.48151314f, -0.752827f,
		0.4487723f, 0f, 0.943001f, 0.16756473f, -0.28752613f, 0f, 0.43480295f, 0.7695305f, -0.46772778f, 0f,
		0.39319962f, 0.5944736f, 0.70142365f, 0f, 0.72543365f, -0.60392565f, 0.33018148f, 0f, 0.75902355f, -0.6506083f,
		0.024333132f, 0f, -0.8552769f, -0.3430043f, 0.38839358f, 0f, -0.6139747f, 0.6981725f, 0.36822575f, 0f,
		-0.74659055f, -0.575201f, 0.33428493f, 0f, 0.5730066f, 0.8105555f, -0.12109168f, 0f, -0.92258775f, -0.3475211f,
		-0.16751404f, 0f, -0.71058166f, -0.47196922f, -0.5218417f, 0f, -0.0856461f, 0.35830015f, 0.9296697f, 0f,
		-0.8279698f, -0.2043157f, 0.5222271f, 0f, 0.42794403f, 0.278166f, 0.8599346f, 0f, 0.539908f, -0.78571206f,
		-0.3019204f, 0f, 0.5678404f, -0.5495414f, -0.61283076f, 0f, -0.9896071f, 0.13656391f, -0.045034185f, 0f,
		-0.6154343f, -0.64408755f, 0.45430374f, 0f, 0.10742044f, -0.79463404f, 0.59750944f, 0f, -0.359545f, -0.888553f,
		0.28495783f, 0f, -0.21804053f, 0.1529889f, 0.9638738f, 0f, -0.7277432f, -0.61640507f, -0.30072346f, 0f,
		0.7249729f, -0.0066971947f, 0.68874484f, 0f, -0.5553659f, -0.5336586f, 0.6377908f, 0f, 0.5137558f, 0.79762083f,
		-0.316f, 0f, -0.3794025f, 0.92456084f, -0.035227515f, 0f, 0.82292485f, 0.27453658f, -0.49741766f, 0f,
		-0.5404114f, 0.60911417f, 0.5804614f, 0f, 0.8036582f, -0.27030295f, 0.5301602f, 0f, 0.60443187f, 0.68329686f,
		0.40959433f, 0f, 0.06389989f, 0.96582085f, -0.2512108f, 0f, 0.10871133f, 0.74024713f, -0.6634878f, 0f,
		-0.7134277f, -0.6926784f, 0.10591285f, 0f, 0.64588976f, -0.57245487f, -0.50509584f, 0f, -0.6553931f, 0.73814714f,
		0.15999562f, 0f, 0.39109614f, 0.91888714f, -0.05186756f, 0f, -0.48790225f, -0.5904377f, 0.64291114f, 0f,
		0.601479f, 0.77074414f, -0.21018201f, 0f, -0.5677173f, 0.7511361f, 0.33688518f, 0f, 0.7858574f, 0.22667466f,
		0.5753667f, 0f, -0.45203456f, -0.6042227f, -0.65618575f, 0f, 0.0022721163f, 0.4132844f, -0.9105992f, 0f,
		-0.58157516f, -0.5162926f, 0.6286591f, 0f, -0.03703705f, 0.8273786f, 0.5604221f, 0f, -0.51196927f, 0.79535437f,
		-0.324498f, 0f, -0.26824173f, -0.957229f, -0.10843876f, 0f, -0.23224828f, -0.9679131f, -0.09594243f, 0f,
		0.3554329f, -0.8881506f, 0.29130062f, 0f, 0.73465204f, -0.4371373f, 0.5188423f, 0f, 0.998512f, 0.046590112f,
		-0.028339446f, 0f, -0.37276876f, -0.9082481f, 0.19007573f, 0f, 0.9173738f, -0.3483642f, 0.19252984f, 0f,
		0.2714911f, 0.41475296f, -0.86848867f, 0f, 0.5131763f, -0.71163344f, 0.4798207f, 0f, -0.87373537f, 0.18886992f,
		-0.44823506f, 0f, 0.84600437f, -0.3725218f, 0.38145f, 0f, 0.89787275f, -0.17802091f, -0.40265754f, 0f,
		0.21780656f, -0.9698323f, -0.10947895f, 0f, -0.15180314f, -0.7788918f, -0.6085091f, 0f, -0.2600385f, -0.4755398f,
		-0.840382f, 0f, 0.5723135f, -0.7474341f, -0.33734185f, 0f, -0.7174141f, 0.16990171f, -0.67561114f, 0f,
		-0.6841808f, 0.021457076f, -0.72899675f, 0f, -0.2007448f, 0.06555606f, -0.9774477f, 0f, -0.11488037f, -0.8044887f,
		0.5827524f, 0f, -0.787035f, 0.03447489f, 0.6159443f, 0f, -0.20155965f, 0.68598723f, 0.69913894f, 0f,
		-0.085810825f, -0.10920836f, -0.99030805f, 0f, 0.5532693f, 0.73252505f, -0.39661077f, 0f, -0.18424894f, -0.9777375f,
		-0.100407675f, 0f, 0.07754738f, -0.9111506f, 0.40471104f, 0f, 0.13998385f, 0.7601631f, -0.63447344f, 0f,
		0.44844192f, -0.84528923f, 0.29049253f, 0f
	};

	private int mSeed;

	private float mFrequency;

	private NoiseType mNoiseType;

	private RotationType3D mRotationType3D;

	private TransformType3D mTransformType3D;

	private FractalType mFractalType;

	private int mOctaves;

	private float mLacunarity;

	private float mGain;

	private float mWeightedStrength;

	private float mPingPongStrength;

	private float mFractalBounding;

	private CellularDistanceFunction mCellularDistanceFunction;

	private CellularReturnType mCellularReturnType;

	private float mCellularJitterModifier;

	private DomainWarpType mDomainWarpType;

	private TransformType3D mWarpTransformType3D;

	private float mDomainWarpAmp;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float FastMin(float a, float b)
	{
		return (a < b) ? a : b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float FastMax(float a, float b)
	{
		return (a > b) ? a : b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float FastAbs(float f)
	{
		return (f < 0f) ? (0f - f) : f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float FastSqrt(float f)
	{
		return (float)Math.Sqrt(f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int FastFloor(double f)
	{
		return (f >= 0.0) ? ((int)f) : ((int)f - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int FastRound(double f)
	{
		return (f >= 0.0) ? ((int)(f + 0.5)) : ((int)(f - 0.5));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float Lerp(float a, float b, float t)
	{
		return a + t * (b - a);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float InterpHermite(float t)
	{
		return t * t * (3f - 2f * t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float InterpQuintic(float t)
	{
		return t * t * t * (t * (t * 6f - 15f) + 10f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float CubicLerp(float a, float b, float c, float d, float t)
	{
		float p = d - c - (a - b);
		return t * t * t * p + t * t * (a - b - p) + t * (c - a) + b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float PingPong(float t)
	{
		t -= (float)((int)(t * 0.5f) * 2);
		return (t < 1f) ? t : (2f - t);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Hash(int seed, int xPrimed, int yPrimed)
	{
		int hash = seed ^ xPrimed ^ yPrimed;
		return hash * 668265261;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int Hash(int seed, int xPrimed, int yPrimed, int zPrimed)
	{
		int hash = seed ^ xPrimed ^ yPrimed ^ zPrimed;
		return hash * 668265261;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float ValCoord(int seed, int xPrimed, int yPrimed)
	{
		int hash = Hash(seed, xPrimed, yPrimed);
		hash *= hash;
		hash ^= hash << 19;
		return (float)hash * 4.656613E-10f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float ValCoord(int seed, int xPrimed, int yPrimed, int zPrimed)
	{
		int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
		hash *= hash;
		hash ^= hash << 19;
		return (float)hash * 4.656613E-10f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GradCoord(int seed, int xPrimed, int yPrimed, float xd, float yd)
	{
		int hash = Hash(seed, xPrimed, yPrimed);
		hash ^= hash >> 15;
		hash &= 0xFE;
		float xg = Gradients2D[hash];
		float yg = Gradients2D[hash | 1];
		return xd * xg + yd * yg;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GradCoord(int seed, int xPrimed, int yPrimed, int zPrimed, float xd, float yd, float zd)
	{
		int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
		hash ^= hash >> 15;
		hash &= 0xFC;
		float xg = Gradients3D[hash];
		float yg = Gradients3D[hash | 1];
		float zg = Gradients3D[hash | 2];
		return xd * xg + yd * yg + zd * zg;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void GradCoordOut(int seed, int xPrimed, int yPrimed, out float xo, out float yo)
	{
		int hash = Hash(seed, xPrimed, yPrimed) & 0x1FE;
		xo = RandVecs2D[hash];
		yo = RandVecs2D[hash | 1];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void GradCoordOut(int seed, int xPrimed, int yPrimed, int zPrimed, out float xo, out float yo, out float zo)
	{
		int hash = Hash(seed, xPrimed, yPrimed, zPrimed) & 0x3FC;
		xo = RandVecs3D[hash];
		yo = RandVecs3D[hash | 1];
		zo = RandVecs3D[hash | 2];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void GradCoordDual(int seed, int xPrimed, int yPrimed, float xd, float yd, out float xo, out float yo)
	{
		int hash = Hash(seed, xPrimed, yPrimed);
		int index1 = hash & 0xFE;
		int index2 = (hash >> 7) & 0x1FE;
		float xg = Gradients2D[index1];
		float yg = Gradients2D[index1 | 1];
		float value = xd * xg + yd * yg;
		float xgo = RandVecs2D[index2];
		float ygo = RandVecs2D[index2 | 1];
		xo = value * xgo;
		yo = value * ygo;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void GradCoordDual(int seed, int xPrimed, int yPrimed, int zPrimed, float xd, float yd, float zd, out float xo, out float yo, out float zo)
	{
		int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
		int index1 = hash & 0xFC;
		int index2 = (hash >> 6) & 0x3FC;
		float xg = Gradients3D[index1];
		float yg = Gradients3D[index1 | 1];
		float zg = Gradients3D[index1 | 2];
		float value = xd * xg + yd * yg + zd * zg;
		float xgo = RandVecs3D[index2];
		float ygo = RandVecs3D[index2 | 1];
		float zgo = RandVecs3D[index2 | 2];
		xo = value * xgo;
		yo = value * ygo;
		zo = value * zgo;
	}

	public FastNoiseLite(int seed = 1337)
	{
		mSeed = seed;
		mFrequency = 0.01f;
		mNoiseType = NoiseType.OpenSimplex2;
		mRotationType3D = RotationType3D.None;
		mTransformType3D = TransformType3D.DefaultOpenSimplex2;
		mFractalType = FractalType.None;
		mOctaves = 3;
		mLacunarity = 2f;
		mGain = 0.5f;
		mWeightedStrength = 0f;
		mPingPongStrength = 2f;
		mFractalBounding = 0.5714286f;
		mCellularDistanceFunction = CellularDistanceFunction.EuclideanSq;
		mCellularReturnType = CellularReturnType.Distance;
		mCellularJitterModifier = 1f;
		mDomainWarpType = DomainWarpType.OpenSimplex2;
		mWarpTransformType3D = TransformType3D.DefaultOpenSimplex2;
		mDomainWarpAmp = 1f;
		SetSeed(seed);
	}

	public void SetSeed(int seed)
	{
		mSeed = seed;
	}

	public void SetFrequency(float frequency)
	{
		mFrequency = frequency;
	}

	public void SetNoiseType(NoiseType noiseType)
	{
		mNoiseType = noiseType;
		UpdateTransformType3D();
	}

	public void SetRotationType3D(RotationType3D rotationType3D)
	{
		mRotationType3D = rotationType3D;
		UpdateTransformType3D();
		UpdateWarpTransformType3D();
	}

	public void SetFractalType(FractalType fractalType)
	{
		mFractalType = fractalType;
	}

	public void SetFractalOctaves(int octaves)
	{
		mOctaves = octaves;
		CalculateFractalBounding();
	}

	public void SetFractalLacunarity(float lacunarity)
	{
		mLacunarity = lacunarity;
	}

	public void SetFractalGain(float gain)
	{
		mGain = gain;
		CalculateFractalBounding();
	}

	public void SetFractalWeightedStrength(float weightedStrength)
	{
		mWeightedStrength = weightedStrength;
	}

	public void SetFractalPingPongStrength(float pingPongStrength)
	{
		mPingPongStrength = pingPongStrength;
	}

	public void SetCellularDistanceFunction(CellularDistanceFunction cellularDistanceFunction)
	{
		mCellularDistanceFunction = cellularDistanceFunction;
	}

	public void SetCellularReturnType(CellularReturnType cellularReturnType)
	{
		mCellularReturnType = cellularReturnType;
	}

	public void SetCellularJitter(float cellularJitter)
	{
		mCellularJitterModifier = cellularJitter;
	}

	public void SetDomainWarpType(DomainWarpType domainWarpType)
	{
		mDomainWarpType = domainWarpType;
		UpdateWarpTransformType3D();
	}

	public void SetDomainWarpAmp(float domainWarpAmp)
	{
		mDomainWarpAmp = domainWarpAmp;
	}

	[MethodImpl((MethodImplOptions)512)]
	public float GetNoise(double x, double y)
	{
		TransformNoiseCoordinate(ref x, ref y);
		return mFractalType switch
		{
			FractalType.FBm => GenFractalFBm(x, y), 
			FractalType.Ridged => GenFractalRidged(x, y), 
			FractalType.PingPong => GenFractalPingPong(x, y), 
			_ => GenNoiseSingle(mSeed, x, y), 
		};
	}

	[MethodImpl((MethodImplOptions)512)]
	public float GetNoise(double x, double y, double z)
	{
		TransformNoiseCoordinate(ref x, ref y, ref z);
		return mFractalType switch
		{
			FractalType.FBm => GenFractalFBm(x, y, z), 
			FractalType.Ridged => GenFractalRidged(x, y, z), 
			FractalType.PingPong => GenFractalPingPong(x, y, z), 
			_ => GenNoiseSingle(mSeed, x, y, z), 
		};
	}

	[MethodImpl((MethodImplOptions)512)]
	public void DomainWarp(ref double x, ref double y)
	{
		switch (mFractalType)
		{
		default:
			DomainWarpSingle(ref x, ref y);
			break;
		case FractalType.DomainWarpProgressive:
			DomainWarpFractalProgressive(ref x, ref y);
			break;
		case FractalType.DomainWarpIndependent:
			DomainWarpFractalIndependent(ref x, ref y);
			break;
		}
	}

	[MethodImpl((MethodImplOptions)512)]
	public void DomainWarp(ref double x, ref double y, ref double z)
	{
		switch (mFractalType)
		{
		default:
			DomainWarpSingle(ref x, ref y, ref z);
			break;
		case FractalType.DomainWarpProgressive:
			DomainWarpFractalProgressive(ref x, ref y, ref z);
			break;
		case FractalType.DomainWarpIndependent:
			DomainWarpFractalIndependent(ref x, ref y, ref z);
			break;
		}
	}

	private void CalculateFractalBounding()
	{
		float gain = FastAbs(mGain);
		float amp = gain;
		float ampFractal = 1f;
		for (int i = 1; i < mOctaves; i++)
		{
			ampFractal += amp;
			amp *= gain;
		}
		mFractalBounding = 1f / ampFractal;
	}

	private float GenNoiseSingle(int seed, double x, double y)
	{
		return mNoiseType switch
		{
			NoiseType.OpenSimplex2 => SingleSimplex(seed, x, y), 
			NoiseType.OpenSimplex2S => SingleOpenSimplex2S(seed, x, y), 
			NoiseType.Cellular => SingleCellular(seed, x, y), 
			NoiseType.Perlin => SinglePerlin(seed, x, y), 
			NoiseType.ValueCubic => SingleValueCubic(seed, x, y), 
			NoiseType.Value => SingleValue(seed, x, y), 
			_ => 0f, 
		};
	}

	private float GenNoiseSingle(int seed, double x, double y, double z)
	{
		return mNoiseType switch
		{
			NoiseType.OpenSimplex2 => SingleOpenSimplex2(seed, x, y, z), 
			NoiseType.OpenSimplex2S => SingleOpenSimplex2S(seed, x, y, z), 
			NoiseType.Cellular => SingleCellular(seed, x, y, z), 
			NoiseType.Perlin => SinglePerlin(seed, x, y, z), 
			NoiseType.ValueCubic => SingleValueCubic(seed, x, y, z), 
			NoiseType.Value => SingleValue(seed, x, y, z), 
			_ => 0f, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void TransformNoiseCoordinate(ref double x, ref double y)
	{
		x *= mFrequency;
		y *= mFrequency;
		NoiseType noiseType = mNoiseType;
		NoiseType noiseType2 = noiseType;
		if ((uint)noiseType2 <= 1u)
		{
			double t = (x + y) * 0.3660254037844386;
			x += t;
			y += t;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void TransformNoiseCoordinate(ref double x, ref double y, ref double z)
	{
		x *= mFrequency;
		y *= mFrequency;
		z *= mFrequency;
		switch (mTransformType3D)
		{
		case TransformType3D.ImproveXYPlanes:
		{
			double xy = x + y;
			double s3 = xy * -0.211324865405187;
			z *= 0.577350269189626;
			x += s3 - z;
			y = y + s3 - z;
			z += xy * 0.577350269189626;
			break;
		}
		case TransformType3D.ImproveXZPlanes:
		{
			double xz = x + z;
			double s2 = xz * -0.211324865405187;
			y *= 0.577350269189626;
			x += s2 - y;
			z += s2 - y;
			y += xz * 0.577350269189626;
			break;
		}
		case TransformType3D.DefaultOpenSimplex2:
		{
			double r = (x + y + z) * (2.0 / 3.0);
			x = r - x;
			y = r - y;
			z = r - z;
			break;
		}
		}
	}

	private void UpdateTransformType3D()
	{
		switch (mRotationType3D)
		{
		case RotationType3D.ImproveXYPlanes:
			mTransformType3D = TransformType3D.ImproveXYPlanes;
			return;
		case RotationType3D.ImproveXZPlanes:
			mTransformType3D = TransformType3D.ImproveXZPlanes;
			return;
		}
		NoiseType noiseType = mNoiseType;
		NoiseType noiseType2 = noiseType;
		if ((uint)noiseType2 <= 1u)
		{
			mTransformType3D = TransformType3D.DefaultOpenSimplex2;
		}
		else
		{
			mTransformType3D = TransformType3D.None;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void TransformDomainWarpCoordinate(ref double x, ref double y)
	{
		DomainWarpType domainWarpType = mDomainWarpType;
		DomainWarpType domainWarpType2 = domainWarpType;
		if ((uint)domainWarpType2 <= 1u)
		{
			double t = (x + y) * 0.3660254037844386;
			x += t;
			y += t;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void TransformDomainWarpCoordinate(ref double x, ref double y, ref double z)
	{
		switch (mWarpTransformType3D)
		{
		case TransformType3D.ImproveXYPlanes:
		{
			double xy = x + y;
			double s3 = xy * -0.211324865405187;
			z *= 0.577350269189626;
			x += s3 - z;
			y = y + s3 - z;
			z += xy * 0.577350269189626;
			break;
		}
		case TransformType3D.ImproveXZPlanes:
		{
			double xz = x + z;
			double s2 = xz * -0.211324865405187;
			y *= 0.577350269189626;
			x += s2 - y;
			z += s2 - y;
			y += xz * 0.577350269189626;
			break;
		}
		case TransformType3D.DefaultOpenSimplex2:
		{
			double r = (x + y + z) * (2.0 / 3.0);
			x = r - x;
			y = r - y;
			z = r - z;
			break;
		}
		}
	}

	private void UpdateWarpTransformType3D()
	{
		switch (mRotationType3D)
		{
		case RotationType3D.ImproveXYPlanes:
			mWarpTransformType3D = TransformType3D.ImproveXYPlanes;
			return;
		case RotationType3D.ImproveXZPlanes:
			mWarpTransformType3D = TransformType3D.ImproveXZPlanes;
			return;
		}
		DomainWarpType domainWarpType = mDomainWarpType;
		DomainWarpType domainWarpType2 = domainWarpType;
		if ((uint)domainWarpType2 <= 1u)
		{
			mWarpTransformType3D = TransformType3D.DefaultOpenSimplex2;
		}
		else
		{
			mWarpTransformType3D = TransformType3D.None;
		}
	}

	private float GenFractalFBm(double x, double y)
	{
		int seed = mSeed;
		float sum = 0f;
		float amp = mFractalBounding;
		for (int i = 0; i < mOctaves; i++)
		{
			float noise = GenNoiseSingle(seed++, x, y);
			sum += noise * amp;
			amp *= Lerp(1f, FastMin(noise + 1f, 2f) * 0.5f, mWeightedStrength);
			x *= (double)mLacunarity;
			y *= (double)mLacunarity;
			amp *= mGain;
		}
		return sum;
	}

	private float GenFractalFBm(double x, double y, double z)
	{
		int seed = mSeed;
		float sum = 0f;
		float amp = mFractalBounding;
		for (int i = 0; i < mOctaves; i++)
		{
			float noise = GenNoiseSingle(seed++, x, y, z);
			sum += noise * amp;
			amp *= Lerp(1f, (noise + 1f) * 0.5f, mWeightedStrength);
			x *= (double)mLacunarity;
			y *= (double)mLacunarity;
			z *= (double)mLacunarity;
			amp *= mGain;
		}
		return sum;
	}

	private float GenFractalRidged(double x, double y)
	{
		int seed = mSeed;
		float sum = 0f;
		float amp = mFractalBounding;
		for (int i = 0; i < mOctaves; i++)
		{
			float noise = FastAbs(GenNoiseSingle(seed++, x, y));
			sum += (noise * -2f + 1f) * amp;
			amp *= Lerp(1f, 1f - noise, mWeightedStrength);
			x *= (double)mLacunarity;
			y *= (double)mLacunarity;
			amp *= mGain;
		}
		return sum;
	}

	private float GenFractalRidged(double x, double y, double z)
	{
		int seed = mSeed;
		float sum = 0f;
		float amp = mFractalBounding;
		for (int i = 0; i < mOctaves; i++)
		{
			float noise = FastAbs(GenNoiseSingle(seed++, x, y, z));
			sum += (noise * -2f + 1f) * amp;
			amp *= Lerp(1f, 1f - noise, mWeightedStrength);
			x *= (double)mLacunarity;
			y *= (double)mLacunarity;
			z *= (double)mLacunarity;
			amp *= mGain;
		}
		return sum;
	}

	private float GenFractalPingPong(double x, double y)
	{
		int seed = mSeed;
		float sum = 0f;
		float amp = mFractalBounding;
		for (int i = 0; i < mOctaves; i++)
		{
			float noise = PingPong((GenNoiseSingle(seed++, x, y) + 1f) * mPingPongStrength);
			sum += (noise - 0.5f) * 2f * amp;
			amp *= Lerp(1f, noise, mWeightedStrength);
			x *= (double)mLacunarity;
			y *= (double)mLacunarity;
			amp *= mGain;
		}
		return sum;
	}

	private float GenFractalPingPong(double x, double y, double z)
	{
		int seed = mSeed;
		float sum = 0f;
		float amp = mFractalBounding;
		for (int i = 0; i < mOctaves; i++)
		{
			float noise = PingPong((GenNoiseSingle(seed++, x, y, z) + 1f) * mPingPongStrength);
			sum += (noise - 0.5f) * 2f * amp;
			amp *= Lerp(1f, noise, mWeightedStrength);
			x *= (double)mLacunarity;
			y *= (double)mLacunarity;
			z *= (double)mLacunarity;
			amp *= mGain;
		}
		return sum;
	}

	private float SingleSimplex(int seed, double x, double y)
	{
		int i = FastFloor(x);
		int j = FastFloor(y);
		float xi = (float)(x - (double)i);
		float yi = (float)(y - (double)j);
		float t = (xi + yi) * 0.21132487f;
		float x2 = xi - t;
		float y2 = yi - t;
		i *= 501125321;
		j *= 1136930381;
		float a = 0.5f - x2 * x2 - y2 * y2;
		float n0 = ((!(a <= 0f)) ? (a * a * (a * a) * GradCoord(seed, i, j, x2, y2)) : 0f);
		float c = 3.1547005f * t + (-0.6666666f + a);
		float n2;
		if (c <= 0f)
		{
			n2 = 0f;
		}
		else
		{
			float x3 = x2 + -0.57735026f;
			float y3 = y2 + -0.57735026f;
			n2 = c * c * (c * c) * GradCoord(seed, i + 501125321, j + 1136930381, x3, y3);
		}
		float n3;
		if (y2 > x2)
		{
			float x4 = x2 + 0.21132487f;
			float y4 = y2 + -0.7886751f;
			float b = 0.5f - x4 * x4 - y4 * y4;
			n3 = ((!(b <= 0f)) ? (b * b * (b * b) * GradCoord(seed, i, j + 1136930381, x4, y4)) : 0f);
		}
		else
		{
			float x5 = x2 + -0.7886751f;
			float y5 = y2 + 0.21132487f;
			float b2 = 0.5f - x5 * x5 - y5 * y5;
			n3 = ((!(b2 <= 0f)) ? (b2 * b2 * (b2 * b2) * GradCoord(seed, i + 501125321, j, x5, y5)) : 0f);
		}
		return (n0 + n3 + n2) * 99.83685f;
	}

	private float SingleOpenSimplex2(int seed, double x, double y, double z)
	{
		int i = FastRound(x);
		int j = FastRound(y);
		int k = FastRound(z);
		float x2 = (float)(x - (double)i);
		float y2 = (float)(y - (double)j);
		float z2 = (float)(z - (double)k);
		int xNSign = (int)(-1f - x2) | 1;
		int yNSign = (int)(-1f - y2) | 1;
		int zNSign = (int)(-1f - z2) | 1;
		float ax0 = (float)xNSign * (0f - x2);
		float ay0 = (float)yNSign * (0f - y2);
		float az0 = (float)zNSign * (0f - z2);
		i *= 501125321;
		j *= 1136930381;
		k *= 1720413743;
		float value = 0f;
		float a = 0.6f - x2 * x2 - (y2 * y2 + z2 * z2);
		int l = 0;
		while (true)
		{
			if (a > 0f)
			{
				value += a * a * (a * a) * GradCoord(seed, i, j, k, x2, y2, z2);
			}
			if (ax0 >= ay0 && ax0 >= az0)
			{
				float b = a + ax0 + ax0;
				if (b > 1f)
				{
					b -= 1f;
					value += b * b * (b * b) * GradCoord(seed, i - xNSign * 501125321, j, k, x2 + (float)xNSign, y2, z2);
				}
			}
			else if (ay0 > ax0 && ay0 >= az0)
			{
				float b2 = a + ay0 + ay0;
				if (b2 > 1f)
				{
					b2 -= 1f;
					value += b2 * b2 * (b2 * b2) * GradCoord(seed, i, j - yNSign * 1136930381, k, x2, y2 + (float)yNSign, z2);
				}
			}
			else
			{
				float b3 = a + az0 + az0;
				if (b3 > 1f)
				{
					b3 -= 1f;
					value += b3 * b3 * (b3 * b3) * GradCoord(seed, i, j, k - zNSign * 1720413743, x2, y2, z2 + (float)zNSign);
				}
			}
			if (l == 1)
			{
				break;
			}
			ax0 = 0.5f - ax0;
			ay0 = 0.5f - ay0;
			az0 = 0.5f - az0;
			x2 = (float)xNSign * ax0;
			y2 = (float)yNSign * ay0;
			z2 = (float)zNSign * az0;
			a += 0.75f - ax0 - (ay0 + az0);
			i += (xNSign >> 1) & 0x1DDE90C9;
			j += (yNSign >> 1) & 0x43C42E4D;
			k += (zNSign >> 1) & 0x668B6E2F;
			xNSign = -xNSign;
			yNSign = -yNSign;
			zNSign = -zNSign;
			seed = ~seed;
			l++;
		}
		return value * 32.694283f;
	}

	private float SingleOpenSimplex2S(int seed, double x, double y)
	{
		int i = FastFloor(x);
		int j = FastFloor(y);
		float xi = (float)(x - (double)i);
		float yi = (float)(y - (double)j);
		i *= 501125321;
		j *= 1136930381;
		int i2 = i + 501125321;
		int j2 = j + 1136930381;
		float t = (xi + yi) * 0.21132487f;
		float x2 = xi - t;
		float y2 = yi - t;
		float a0 = 2f / 3f - x2 * x2 - y2 * y2;
		float value = a0 * a0 * (a0 * a0) * GradCoord(seed, i, j, x2, y2);
		float a1 = 3.1547005f * t + (-2f / 3f + a0);
		float x3 = x2 - 0.57735026f;
		float y3 = y2 - 0.57735026f;
		value += a1 * a1 * (a1 * a1) * GradCoord(seed, i2, j2, x3, y3);
		float xmyi = xi - yi;
		if ((double)t > 0.21132486540518713)
		{
			if (xi + xmyi > 1f)
			{
				float x4 = x2 + -1.3660254f;
				float y4 = y2 + -0.36602542f;
				float a2 = 2f / 3f - x4 * x4 - y4 * y4;
				if (a2 > 0f)
				{
					value += a2 * a2 * (a2 * a2) * GradCoord(seed, i + 1002250642, j + 1136930381, x4, y4);
				}
			}
			else
			{
				float x5 = x2 + 0.21132487f;
				float y5 = y2 + -0.7886751f;
				float a3 = 2f / 3f - x5 * x5 - y5 * y5;
				if (a3 > 0f)
				{
					value += a3 * a3 * (a3 * a3) * GradCoord(seed, i, j + 1136930381, x5, y5);
				}
			}
			if (yi - xmyi > 1f)
			{
				float x6 = x2 + -0.36602542f;
				float y6 = y2 + -1.3660254f;
				float a4 = 2f / 3f - x6 * x6 - y6 * y6;
				if (a4 > 0f)
				{
					value += a4 * a4 * (a4 * a4) * GradCoord(seed, i + 501125321, j + -2021106534, x6, y6);
				}
			}
			else
			{
				float x7 = x2 + -0.7886751f;
				float y7 = y2 + 0.21132487f;
				float a5 = 2f / 3f - x7 * x7 - y7 * y7;
				if (a5 > 0f)
				{
					value += a5 * a5 * (a5 * a5) * GradCoord(seed, i + 501125321, j, x7, y7);
				}
			}
		}
		else
		{
			if (xi + xmyi < 0f)
			{
				float x8 = x2 + 0.7886751f;
				float y8 = y2 - 0.21132487f;
				float a6 = 2f / 3f - x8 * x8 - y8 * y8;
				if (a6 > 0f)
				{
					value += a6 * a6 * (a6 * a6) * GradCoord(seed, i - 501125321, j, x8, y8);
				}
			}
			else
			{
				float x9 = x2 + -0.7886751f;
				float y9 = y2 + 0.21132487f;
				float a7 = 2f / 3f - x9 * x9 - y9 * y9;
				if (a7 > 0f)
				{
					value += a7 * a7 * (a7 * a7) * GradCoord(seed, i + 501125321, j, x9, y9);
				}
			}
			if (yi < xmyi)
			{
				float x10 = x2 - 0.21132487f;
				float y10 = y2 - -0.7886751f;
				float a8 = 2f / 3f - x10 * x10 - y10 * y10;
				if (a8 > 0f)
				{
					value += a8 * a8 * (a8 * a8) * GradCoord(seed, i, j - 1136930381, x10, y10);
				}
			}
			else
			{
				float x11 = x2 + 0.21132487f;
				float y11 = y2 + -0.7886751f;
				float a9 = 2f / 3f - x11 * x11 - y11 * y11;
				if (a9 > 0f)
				{
					value += a9 * a9 * (a9 * a9) * GradCoord(seed, i, j + 1136930381, x11, y11);
				}
			}
		}
		return value * 18.241962f;
	}

	private float SingleOpenSimplex2S(int seed, double x, double y, double z)
	{
		int i = FastFloor(x);
		int j = FastFloor(y);
		int k = FastFloor(z);
		float xi = (float)(x - (double)i);
		float yi = (float)(y - (double)j);
		float zi = (float)(z - (double)k);
		i *= 501125321;
		j *= 1136930381;
		k *= 1720413743;
		int seed2 = seed + 1293373;
		int xNMask = (int)(-0.5f - xi);
		int yNMask = (int)(-0.5f - yi);
		int zNMask = (int)(-0.5f - zi);
		float x2 = xi + (float)xNMask;
		float y2 = yi + (float)yNMask;
		float z2 = zi + (float)zNMask;
		float a0 = 0.75f - x2 * x2 - y2 * y2 - z2 * z2;
		float value = a0 * a0 * (a0 * a0) * GradCoord(seed, i + (xNMask & 0x1DDE90C9), j + (yNMask & 0x43C42E4D), k + (zNMask & 0x668B6E2F), x2, y2, z2);
		float x3 = xi - 0.5f;
		float y3 = yi - 0.5f;
		float z3 = zi - 0.5f;
		float a1 = 0.75f - x3 * x3 - y3 * y3 - z3 * z3;
		value += a1 * a1 * (a1 * a1) * GradCoord(seed2, i + 501125321, j + 1136930381, k + 1720413743, x3, y3, z3);
		float xAFlipMask0 = (float)((xNMask | 1) << 1) * x3;
		float yAFlipMask0 = (float)((yNMask | 1) << 1) * y3;
		float zAFlipMask0 = (float)((zNMask | 1) << 1) * z3;
		float xAFlipMask1 = (float)(-2 - (xNMask << 2)) * x3 - 1f;
		float yAFlipMask1 = (float)(-2 - (yNMask << 2)) * y3 - 1f;
		float zAFlipMask1 = (float)(-2 - (zNMask << 2)) * z3 - 1f;
		bool skip5 = false;
		float a2 = xAFlipMask0 + a0;
		if (a2 > 0f)
		{
			float x4 = x2 - (float)(xNMask | 1);
			float y4 = y2;
			float z4 = z2;
			value += a2 * a2 * (a2 * a2) * GradCoord(seed, i + (~xNMask & 0x1DDE90C9), j + (yNMask & 0x43C42E4D), k + (zNMask & 0x668B6E2F), x4, y4, z4);
		}
		else
		{
			float a3 = yAFlipMask0 + zAFlipMask0 + a0;
			if (a3 > 0f)
			{
				float x5 = x2;
				float y5 = y2 - (float)(yNMask | 1);
				float z5 = z2 - (float)(zNMask | 1);
				value += a3 * a3 * (a3 * a3) * GradCoord(seed, i + (xNMask & 0x1DDE90C9), j + (~yNMask & 0x43C42E4D), k + (~zNMask & 0x668B6E2F), x5, y5, z5);
			}
			float a4 = xAFlipMask1 + a1;
			if (a4 > 0f)
			{
				float x6 = (float)(xNMask | 1) + x3;
				float y6 = y3;
				float z6 = z3;
				value += a4 * a4 * (a4 * a4) * GradCoord(seed2, i + (xNMask & 0x3BBD2192), j + 1136930381, k + 1720413743, x6, y6, z6);
				skip5 = true;
			}
		}
		bool skip9 = false;
		float a6 = yAFlipMask0 + a0;
		if (a6 > 0f)
		{
			float x7 = x2;
			float y7 = y2 - (float)(yNMask | 1);
			float z7 = z2;
			value += a6 * a6 * (a6 * a6) * GradCoord(seed, i + (xNMask & 0x1DDE90C9), j + (~yNMask & 0x43C42E4D), k + (zNMask & 0x668B6E2F), x7, y7, z7);
		}
		else
		{
			float a7 = xAFlipMask0 + zAFlipMask0 + a0;
			if (a7 > 0f)
			{
				float x8 = x2 - (float)(xNMask | 1);
				float y8 = y2;
				float z8 = z2 - (float)(zNMask | 1);
				value += a7 * a7 * (a7 * a7) * GradCoord(seed, i + (~xNMask & 0x1DDE90C9), j + (yNMask & 0x43C42E4D), k + (~zNMask & 0x668B6E2F), x8, y8, z8);
			}
			float a8 = yAFlipMask1 + a1;
			if (a8 > 0f)
			{
				float x9 = x3;
				float y9 = (float)(yNMask | 1) + y3;
				float z9 = z3;
				value += a8 * a8 * (a8 * a8) * GradCoord(seed2, i + 501125321, j + (yNMask & -2021106534), k + 1720413743, x9, y9, z9);
				skip9 = true;
			}
		}
		bool skipD = false;
		float aA = zAFlipMask0 + a0;
		if (aA > 0f)
		{
			float xA = x2;
			float yA = y2;
			float zA = z2 - (float)(zNMask | 1);
			value += aA * aA * (aA * aA) * GradCoord(seed, i + (xNMask & 0x1DDE90C9), j + (yNMask & 0x43C42E4D), k + (~zNMask & 0x668B6E2F), xA, yA, zA);
		}
		else
		{
			float aB = xAFlipMask0 + yAFlipMask0 + a0;
			if (aB > 0f)
			{
				float xB = x2 - (float)(xNMask | 1);
				float yB = y2 - (float)(yNMask | 1);
				float zB = z2;
				value += aB * aB * (aB * aB) * GradCoord(seed, i + (~xNMask & 0x1DDE90C9), j + (~yNMask & 0x43C42E4D), k + (zNMask & 0x668B6E2F), xB, yB, zB);
			}
			float aC = zAFlipMask1 + a1;
			if (aC > 0f)
			{
				float xC = x3;
				float yC = y3;
				float zC = (float)(zNMask | 1) + z3;
				value += aC * aC * (aC * aC) * GradCoord(seed2, i + 501125321, j + 1136930381, k + (zNMask & -854139810), xC, yC, zC);
				skipD = true;
			}
		}
		if (!skip5)
		{
			float a9 = yAFlipMask1 + zAFlipMask1 + a1;
			if (a9 > 0f)
			{
				float x10 = x3;
				float y10 = (float)(yNMask | 1) + y3;
				float z10 = (float)(zNMask | 1) + z3;
				value += a9 * a9 * (a9 * a9) * GradCoord(seed2, i + 501125321, j + (yNMask & -2021106534), k + (zNMask & -854139810), x10, y10, z10);
			}
		}
		if (!skip9)
		{
			float a10 = xAFlipMask1 + zAFlipMask1 + a1;
			if (a10 > 0f)
			{
				float x11 = (float)(xNMask | 1) + x3;
				float y11 = y3;
				float z11 = (float)(zNMask | 1) + z3;
				value += a10 * a10 * (a10 * a10) * GradCoord(seed2, i + (xNMask & 0x3BBD2192), j + 1136930381, k + (zNMask & -854139810), x11, y11, z11);
			}
		}
		if (!skipD)
		{
			float aD = xAFlipMask1 + yAFlipMask1 + a1;
			if (aD > 0f)
			{
				float xD = (float)(xNMask | 1) + x3;
				float yD = (float)(yNMask | 1) + y3;
				float zD = z3;
				value += aD * aD * (aD * aD) * GradCoord(seed2, i + (xNMask & 0x3BBD2192), j + (yNMask & -2021106534), k + 1720413743, xD, yD, zD);
			}
		}
		return value * 9.046026f;
	}

	private float SingleCellular(int seed, double x, double y)
	{
		int xr = FastRound(x);
		int yr = FastRound(y);
		float distance0 = float.MaxValue;
		float distance1 = float.MaxValue;
		int closestHash = 0;
		float cellularJitter = 0.43701595f * mCellularJitterModifier;
		int xPrimed = (xr - 1) * 501125321;
		int yPrimedBase = (yr - 1) * 1136930381;
		switch (mCellularDistanceFunction)
		{
		default:
		{
			for (int i = xr - 1; i <= xr + 1; i++)
			{
				int yPrimed2 = yPrimedBase;
				for (int j = yr - 1; j <= yr + 1; j++)
				{
					int hash2 = Hash(seed, xPrimed, yPrimed2);
					int idx2 = hash2 & 0x1FE;
					float vecX2 = (float)((double)i - x) + RandVecs2D[idx2] * cellularJitter;
					float vecY2 = (float)((double)j - y) + RandVecs2D[idx2 | 1] * cellularJitter;
					float newDistance2 = vecX2 * vecX2 + vecY2 * vecY2;
					distance1 = FastMax(FastMin(distance1, newDistance2), distance0);
					if (newDistance2 < distance0)
					{
						distance0 = newDistance2;
						closestHash = hash2;
					}
					yPrimed2 += 1136930381;
				}
				xPrimed += 501125321;
			}
			break;
		}
		case CellularDistanceFunction.Manhattan:
		{
			for (int k = xr - 1; k <= xr + 1; k++)
			{
				int yPrimed3 = yPrimedBase;
				for (int l = yr - 1; l <= yr + 1; l++)
				{
					int hash3 = Hash(seed, xPrimed, yPrimed3);
					int idx3 = hash3 & 0x1FE;
					float vecX3 = (float)((double)k - x) + RandVecs2D[idx3] * cellularJitter;
					float vecY3 = (float)((double)l - y) + RandVecs2D[idx3 | 1] * cellularJitter;
					float newDistance3 = FastAbs(vecX3) + FastAbs(vecY3);
					distance1 = FastMax(FastMin(distance1, newDistance3), distance0);
					if (newDistance3 < distance0)
					{
						distance0 = newDistance3;
						closestHash = hash3;
					}
					yPrimed3 += 1136930381;
				}
				xPrimed += 501125321;
			}
			break;
		}
		case CellularDistanceFunction.Hybrid:
		{
			for (int xi = xr - 1; xi <= xr + 1; xi++)
			{
				int yPrimed = yPrimedBase;
				for (int yi = yr - 1; yi <= yr + 1; yi++)
				{
					int hash = Hash(seed, xPrimed, yPrimed);
					int idx = hash & 0x1FE;
					float vecX = (float)((double)xi - x) + RandVecs2D[idx] * cellularJitter;
					float vecY = (float)((double)yi - y) + RandVecs2D[idx | 1] * cellularJitter;
					float newDistance = FastAbs(vecX) + FastAbs(vecY) + (vecX * vecX + vecY * vecY);
					distance1 = FastMax(FastMin(distance1, newDistance), distance0);
					if (newDistance < distance0)
					{
						distance0 = newDistance;
						closestHash = hash;
					}
					yPrimed += 1136930381;
				}
				xPrimed += 501125321;
			}
			break;
		}
		}
		if (mCellularDistanceFunction == CellularDistanceFunction.Euclidean && mCellularReturnType >= CellularReturnType.Distance)
		{
			distance0 = FastSqrt(distance0);
			if (mCellularReturnType >= CellularReturnType.Distance2)
			{
				distance1 = FastSqrt(distance1);
			}
		}
		return mCellularReturnType switch
		{
			CellularReturnType.CellValue => (float)closestHash * 4.656613E-10f, 
			CellularReturnType.Distance => distance0 - 1f, 
			CellularReturnType.Distance2 => distance1 - 1f, 
			CellularReturnType.Distance2Add => (distance1 + distance0) * 0.5f - 1f, 
			CellularReturnType.Distance2Sub => distance1 - distance0 - 1f, 
			CellularReturnType.Distance2Mul => distance1 * distance0 * 0.5f - 1f, 
			CellularReturnType.Distance2Div => distance0 / distance1 - 1f, 
			_ => 0f, 
		};
	}

	private float SingleCellular(int seed, double x, double y, double z)
	{
		int xr = FastRound(x);
		int yr = FastRound(y);
		int zr = FastRound(z);
		float distance0 = float.MaxValue;
		float distance1 = float.MaxValue;
		int closestHash = 0;
		float cellularJitter = 0.39614353f * mCellularJitterModifier;
		int xPrimed = (xr - 1) * 501125321;
		int yPrimedBase = (yr - 1) * 1136930381;
		int zPrimedBase = (zr - 1) * 1720413743;
		switch (mCellularDistanceFunction)
		{
		case CellularDistanceFunction.Euclidean:
		case CellularDistanceFunction.EuclideanSq:
		{
			for (int i = xr - 1; i <= xr + 1; i++)
			{
				int yPrimed2 = yPrimedBase;
				for (int j = yr - 1; j <= yr + 1; j++)
				{
					int zPrimed2 = zPrimedBase;
					for (int k = zr - 1; k <= zr + 1; k++)
					{
						int hash2 = Hash(seed, xPrimed, yPrimed2, zPrimed2);
						int idx2 = hash2 & 0x3FC;
						float vecX2 = (float)((double)i - x) + RandVecs3D[idx2] * cellularJitter;
						float vecY2 = (float)((double)j - y) + RandVecs3D[idx2 | 1] * cellularJitter;
						float vecZ2 = (float)((double)k - z) + RandVecs3D[idx2 | 2] * cellularJitter;
						float newDistance2 = vecX2 * vecX2 + vecY2 * vecY2 + vecZ2 * vecZ2;
						distance1 = FastMax(FastMin(distance1, newDistance2), distance0);
						if (newDistance2 < distance0)
						{
							distance0 = newDistance2;
							closestHash = hash2;
						}
						zPrimed2 += 1720413743;
					}
					yPrimed2 += 1136930381;
				}
				xPrimed += 501125321;
			}
			break;
		}
		case CellularDistanceFunction.Manhattan:
		{
			for (int l = xr - 1; l <= xr + 1; l++)
			{
				int yPrimed3 = yPrimedBase;
				for (int m = yr - 1; m <= yr + 1; m++)
				{
					int zPrimed3 = zPrimedBase;
					for (int n = zr - 1; n <= zr + 1; n++)
					{
						int hash3 = Hash(seed, xPrimed, yPrimed3, zPrimed3);
						int idx3 = hash3 & 0x3FC;
						float vecX3 = (float)((double)l - x) + RandVecs3D[idx3] * cellularJitter;
						float vecY3 = (float)((double)m - y) + RandVecs3D[idx3 | 1] * cellularJitter;
						float vecZ3 = (float)((double)n - z) + RandVecs3D[idx3 | 2] * cellularJitter;
						float newDistance3 = FastAbs(vecX3) + FastAbs(vecY3) + FastAbs(vecZ3);
						distance1 = FastMax(FastMin(distance1, newDistance3), distance0);
						if (newDistance3 < distance0)
						{
							distance0 = newDistance3;
							closestHash = hash3;
						}
						zPrimed3 += 1720413743;
					}
					yPrimed3 += 1136930381;
				}
				xPrimed += 501125321;
			}
			break;
		}
		case CellularDistanceFunction.Hybrid:
		{
			for (int xi = xr - 1; xi <= xr + 1; xi++)
			{
				int yPrimed = yPrimedBase;
				for (int yi = yr - 1; yi <= yr + 1; yi++)
				{
					int zPrimed = zPrimedBase;
					for (int zi = zr - 1; zi <= zr + 1; zi++)
					{
						int hash = Hash(seed, xPrimed, yPrimed, zPrimed);
						int idx = hash & 0x3FC;
						float vecX = (float)((double)xi - x) + RandVecs3D[idx] * cellularJitter;
						float vecY = (float)((double)yi - y) + RandVecs3D[idx | 1] * cellularJitter;
						float vecZ = (float)((double)zi - z) + RandVecs3D[idx | 2] * cellularJitter;
						float newDistance = FastAbs(vecX) + FastAbs(vecY) + FastAbs(vecZ) + (vecX * vecX + vecY * vecY + vecZ * vecZ);
						distance1 = FastMax(FastMin(distance1, newDistance), distance0);
						if (newDistance < distance0)
						{
							distance0 = newDistance;
							closestHash = hash;
						}
						zPrimed += 1720413743;
					}
					yPrimed += 1136930381;
				}
				xPrimed += 501125321;
			}
			break;
		}
		}
		if (mCellularDistanceFunction == CellularDistanceFunction.Euclidean && mCellularReturnType >= CellularReturnType.Distance)
		{
			distance0 = FastSqrt(distance0);
			if (mCellularReturnType >= CellularReturnType.Distance2)
			{
				distance1 = FastSqrt(distance1);
			}
		}
		return mCellularReturnType switch
		{
			CellularReturnType.CellValue => (float)closestHash * 4.656613E-10f, 
			CellularReturnType.Distance => distance0 - 1f, 
			CellularReturnType.Distance2 => distance1 - 1f, 
			CellularReturnType.Distance2Add => (distance1 + distance0) * 0.5f - 1f, 
			CellularReturnType.Distance2Sub => distance1 - distance0 - 1f, 
			CellularReturnType.Distance2Mul => distance1 * distance0 * 0.5f - 1f, 
			CellularReturnType.Distance2Div => distance0 / distance1 - 1f, 
			_ => 0f, 
		};
	}

	private float SinglePerlin(int seed, double x, double y)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		float xd0 = (float)(x - (double)x2);
		float yd0 = (float)(y - (double)y2);
		float xd1 = xd0 - 1f;
		float yd1 = yd0 - 1f;
		float xs = InterpQuintic(xd0);
		float ys = InterpQuintic(yd0);
		x2 *= 501125321;
		y2 *= 1136930381;
		int x3 = x2 + 501125321;
		int y3 = y2 + 1136930381;
		float xf0 = Lerp(GradCoord(seed, x2, y2, xd0, yd0), GradCoord(seed, x3, y2, xd1, yd0), xs);
		float xf1 = Lerp(GradCoord(seed, x2, y3, xd0, yd1), GradCoord(seed, x3, y3, xd1, yd1), xs);
		return Lerp(xf0, xf1, ys) * 1.4247692f;
	}

	private float SinglePerlin(int seed, double x, double y, double z)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int z2 = FastFloor(z);
		float xd0 = (float)(x - (double)x2);
		float yd0 = (float)(y - (double)y2);
		float zd0 = (float)(z - (double)z2);
		float xd1 = xd0 - 1f;
		float yd1 = yd0 - 1f;
		float zd1 = zd0 - 1f;
		float xs = InterpQuintic(xd0);
		float ys = InterpQuintic(yd0);
		float zs = InterpQuintic(zd0);
		x2 *= 501125321;
		y2 *= 1136930381;
		z2 *= 1720413743;
		int x3 = x2 + 501125321;
		int y3 = y2 + 1136930381;
		int z3 = z2 + 1720413743;
		float xf00 = Lerp(GradCoord(seed, x2, y2, z2, xd0, yd0, zd0), GradCoord(seed, x3, y2, z2, xd1, yd0, zd0), xs);
		float xf10 = Lerp(GradCoord(seed, x2, y3, z2, xd0, yd1, zd0), GradCoord(seed, x3, y3, z2, xd1, yd1, zd0), xs);
		float xf11 = Lerp(GradCoord(seed, x2, y2, z3, xd0, yd0, zd1), GradCoord(seed, x3, y2, z3, xd1, yd0, zd1), xs);
		float xf12 = Lerp(GradCoord(seed, x2, y3, z3, xd0, yd1, zd1), GradCoord(seed, x3, y3, z3, xd1, yd1, zd1), xs);
		float yf0 = Lerp(xf00, xf10, ys);
		float yf1 = Lerp(xf11, xf12, ys);
		return Lerp(yf0, yf1, zs) * 0.9649214f;
	}

	private float SingleValueCubic(int seed, double x, double y)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		float xs = (float)(x - (double)x2);
		float ys = (float)(y - (double)y2);
		x2 *= 501125321;
		y2 *= 1136930381;
		int x3 = x2 - 501125321;
		int y3 = y2 - 1136930381;
		int x4 = x2 + 501125321;
		int y4 = y2 + 1136930381;
		int x5 = x2 + 1002250642;
		int y5 = y2 + -2021106534;
		return CubicLerp(CubicLerp(ValCoord(seed, x3, y3), ValCoord(seed, x2, y3), ValCoord(seed, x4, y3), ValCoord(seed, x5, y3), xs), CubicLerp(ValCoord(seed, x3, y2), ValCoord(seed, x2, y2), ValCoord(seed, x4, y2), ValCoord(seed, x5, y2), xs), CubicLerp(ValCoord(seed, x3, y4), ValCoord(seed, x2, y4), ValCoord(seed, x4, y4), ValCoord(seed, x5, y4), xs), CubicLerp(ValCoord(seed, x3, y5), ValCoord(seed, x2, y5), ValCoord(seed, x4, y5), ValCoord(seed, x5, y5), xs), ys) * (4f / 9f);
	}

	private float SingleValueCubic(int seed, double x, double y, double z)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int z2 = FastFloor(z);
		float xs = (float)(x - (double)x2);
		float ys = (float)(y - (double)y2);
		float zs = (float)(z - (double)z2);
		x2 *= 501125321;
		y2 *= 1136930381;
		z2 *= 1720413743;
		int x3 = x2 - 501125321;
		int y3 = y2 - 1136930381;
		int z3 = z2 - 1720413743;
		int x4 = x2 + 501125321;
		int y4 = y2 + 1136930381;
		int z4 = z2 + 1720413743;
		int x5 = x2 + 1002250642;
		int y5 = y2 + -2021106534;
		int z5 = z2 + -854139810;
		return CubicLerp(CubicLerp(CubicLerp(ValCoord(seed, x3, y3, z3), ValCoord(seed, x2, y3, z3), ValCoord(seed, x4, y3, z3), ValCoord(seed, x5, y3, z3), xs), CubicLerp(ValCoord(seed, x3, y2, z3), ValCoord(seed, x2, y2, z3), ValCoord(seed, x4, y2, z3), ValCoord(seed, x5, y2, z3), xs), CubicLerp(ValCoord(seed, x3, y4, z3), ValCoord(seed, x2, y4, z3), ValCoord(seed, x4, y4, z3), ValCoord(seed, x5, y4, z3), xs), CubicLerp(ValCoord(seed, x3, y5, z3), ValCoord(seed, x2, y5, z3), ValCoord(seed, x4, y5, z3), ValCoord(seed, x5, y5, z3), xs), ys), CubicLerp(CubicLerp(ValCoord(seed, x3, y3, z2), ValCoord(seed, x2, y3, z2), ValCoord(seed, x4, y3, z2), ValCoord(seed, x5, y3, z2), xs), CubicLerp(ValCoord(seed, x3, y2, z2), ValCoord(seed, x2, y2, z2), ValCoord(seed, x4, y2, z2), ValCoord(seed, x5, y2, z2), xs), CubicLerp(ValCoord(seed, x3, y4, z2), ValCoord(seed, x2, y4, z2), ValCoord(seed, x4, y4, z2), ValCoord(seed, x5, y4, z2), xs), CubicLerp(ValCoord(seed, x3, y5, z2), ValCoord(seed, x2, y5, z2), ValCoord(seed, x4, y5, z2), ValCoord(seed, x5, y5, z2), xs), ys), CubicLerp(CubicLerp(ValCoord(seed, x3, y3, z4), ValCoord(seed, x2, y3, z4), ValCoord(seed, x4, y3, z4), ValCoord(seed, x5, y3, z4), xs), CubicLerp(ValCoord(seed, x3, y2, z4), ValCoord(seed, x2, y2, z4), ValCoord(seed, x4, y2, z4), ValCoord(seed, x5, y2, z4), xs), CubicLerp(ValCoord(seed, x3, y4, z4), ValCoord(seed, x2, y4, z4), ValCoord(seed, x4, y4, z4), ValCoord(seed, x5, y4, z4), xs), CubicLerp(ValCoord(seed, x3, y5, z4), ValCoord(seed, x2, y5, z4), ValCoord(seed, x4, y5, z4), ValCoord(seed, x5, y5, z4), xs), ys), CubicLerp(CubicLerp(ValCoord(seed, x3, y3, z5), ValCoord(seed, x2, y3, z5), ValCoord(seed, x4, y3, z5), ValCoord(seed, x5, y3, z5), xs), CubicLerp(ValCoord(seed, x3, y2, z5), ValCoord(seed, x2, y2, z5), ValCoord(seed, x4, y2, z5), ValCoord(seed, x5, y2, z5), xs), CubicLerp(ValCoord(seed, x3, y4, z5), ValCoord(seed, x2, y4, z5), ValCoord(seed, x4, y4, z5), ValCoord(seed, x5, y4, z5), xs), CubicLerp(ValCoord(seed, x3, y5, z5), ValCoord(seed, x2, y5, z5), ValCoord(seed, x4, y5, z5), ValCoord(seed, x5, y5, z5), xs), ys), zs) * (8f / 27f);
	}

	private float SingleValue(int seed, double x, double y)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		float xs = InterpHermite((float)(x - (double)x2));
		float ys = InterpHermite((float)(y - (double)y2));
		x2 *= 501125321;
		y2 *= 1136930381;
		int x3 = x2 + 501125321;
		int y3 = y2 + 1136930381;
		float xf0 = Lerp(ValCoord(seed, x2, y2), ValCoord(seed, x3, y2), xs);
		float xf1 = Lerp(ValCoord(seed, x2, y3), ValCoord(seed, x3, y3), xs);
		return Lerp(xf0, xf1, ys);
	}

	private float SingleValue(int seed, double x, double y, double z)
	{
		int x2 = FastFloor(x);
		int y2 = FastFloor(y);
		int z2 = FastFloor(z);
		float xs = InterpHermite((float)(x - (double)x2));
		float ys = InterpHermite((float)(y - (double)y2));
		float zs = InterpHermite((float)(z - (double)z2));
		x2 *= 501125321;
		y2 *= 1136930381;
		z2 *= 1720413743;
		int x3 = x2 + 501125321;
		int y3 = y2 + 1136930381;
		int z3 = z2 + 1720413743;
		float xf00 = Lerp(ValCoord(seed, x2, y2, z2), ValCoord(seed, x3, y2, z2), xs);
		float xf10 = Lerp(ValCoord(seed, x2, y3, z2), ValCoord(seed, x3, y3, z2), xs);
		float xf11 = Lerp(ValCoord(seed, x2, y2, z3), ValCoord(seed, x3, y2, z3), xs);
		float xf12 = Lerp(ValCoord(seed, x2, y3, z3), ValCoord(seed, x3, y3, z3), xs);
		float yf0 = Lerp(xf00, xf10, ys);
		float yf1 = Lerp(xf11, xf12, ys);
		return Lerp(yf0, yf1, zs);
	}

	private void DoSingleDomainWarp(int seed, float amp, float freq, double x, double y, ref double xr, ref double yr)
	{
		switch (mDomainWarpType)
		{
		case DomainWarpType.OpenSimplex2:
			SingleDomainWarpSimplexGradient(seed, amp * 38.283688f, freq, x, y, ref xr, ref yr, outGradOnly: false);
			break;
		case DomainWarpType.OpenSimplex2Reduced:
			SingleDomainWarpSimplexGradient(seed, amp * 16f, freq, x, y, ref xr, ref yr, outGradOnly: true);
			break;
		case DomainWarpType.BasicGrid:
			SingleDomainWarpBasicGrid(seed, amp, freq, x, y, ref xr, ref yr);
			break;
		}
	}

	private void DoSingleDomainWarp(int seed, float amp, float freq, double x, double y, double z, ref double xr, ref double yr, ref double zr)
	{
		switch (mDomainWarpType)
		{
		case DomainWarpType.OpenSimplex2:
			SingleDomainWarpOpenSimplex2Gradient(seed, amp * 32.694283f, freq, x, y, z, ref xr, ref yr, ref zr, outGradOnly: false);
			break;
		case DomainWarpType.OpenSimplex2Reduced:
			SingleDomainWarpOpenSimplex2Gradient(seed, amp * 7.716049f, freq, x, y, z, ref xr, ref yr, ref zr, outGradOnly: true);
			break;
		case DomainWarpType.BasicGrid:
			SingleDomainWarpBasicGrid(seed, amp, freq, x, y, z, ref xr, ref yr, ref zr);
			break;
		}
	}

	private void DomainWarpSingle(ref double x, ref double y)
	{
		int seed = mSeed;
		float amp = mDomainWarpAmp * mFractalBounding;
		float freq = mFrequency;
		double xs = x;
		double ys = y;
		TransformDomainWarpCoordinate(ref xs, ref ys);
		DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);
	}

	private void DomainWarpSingle(ref double x, ref double y, ref double z)
	{
		int seed = mSeed;
		float amp = mDomainWarpAmp * mFractalBounding;
		float freq = mFrequency;
		double xs = x;
		double ys = y;
		double zs = z;
		TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);
		DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);
	}

	private void DomainWarpFractalProgressive(ref double x, ref double y)
	{
		int seed = mSeed;
		float amp = mDomainWarpAmp * mFractalBounding;
		float freq = mFrequency;
		for (int i = 0; i < mOctaves; i++)
		{
			double xs = x;
			double ys = y;
			TransformDomainWarpCoordinate(ref xs, ref ys);
			DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);
			seed++;
			amp *= mGain;
			freq *= mLacunarity;
		}
	}

	private void DomainWarpFractalProgressive(ref double x, ref double y, ref double z)
	{
		int seed = mSeed;
		float amp = mDomainWarpAmp * mFractalBounding;
		float freq = mFrequency;
		for (int i = 0; i < mOctaves; i++)
		{
			double xs = x;
			double ys = y;
			double zs = z;
			TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);
			DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);
			seed++;
			amp *= mGain;
			freq *= mLacunarity;
		}
	}

	private void DomainWarpFractalIndependent(ref double x, ref double y)
	{
		double xs = x;
		double ys = y;
		TransformDomainWarpCoordinate(ref xs, ref ys);
		int seed = mSeed;
		float amp = mDomainWarpAmp * mFractalBounding;
		float freq = mFrequency;
		for (int i = 0; i < mOctaves; i++)
		{
			DoSingleDomainWarp(seed, amp, freq, xs, ys, ref x, ref y);
			seed++;
			amp *= mGain;
			freq *= mLacunarity;
		}
	}

	private void DomainWarpFractalIndependent(ref double x, ref double y, ref double z)
	{
		double xs = x;
		double ys = y;
		double zs = z;
		TransformDomainWarpCoordinate(ref xs, ref ys, ref zs);
		int seed = mSeed;
		float amp = mDomainWarpAmp * mFractalBounding;
		float freq = mFrequency;
		for (int i = 0; i < mOctaves; i++)
		{
			DoSingleDomainWarp(seed, amp, freq, xs, ys, zs, ref x, ref y, ref z);
			seed++;
			amp *= mGain;
			freq *= mLacunarity;
		}
	}

	private void SingleDomainWarpBasicGrid(int seed, float warpAmp, float frequency, double x, double y, ref double xr, ref double yr)
	{
		double xf = x * (double)frequency;
		double yf = y * (double)frequency;
		int x2 = FastFloor(xf);
		int y2 = FastFloor(yf);
		float xs = InterpHermite((float)(xf - (double)x2));
		float ys = InterpHermite((float)(yf - (double)y2));
		x2 *= 501125321;
		y2 *= 1136930381;
		int x3 = x2 + 501125321;
		int y3 = y2 + 1136930381;
		int hash0 = Hash(seed, x2, y2) & 0x1FE;
		int hash1 = Hash(seed, x3, y2) & 0x1FE;
		float lx0x = Lerp(RandVecs2D[hash0], RandVecs2D[hash1], xs);
		float ly0x = Lerp(RandVecs2D[hash0 | 1], RandVecs2D[hash1 | 1], xs);
		hash0 = Hash(seed, x2, y3) & 0x1FE;
		hash1 = Hash(seed, x3, y3) & 0x1FE;
		float lx1x = Lerp(RandVecs2D[hash0], RandVecs2D[hash1], xs);
		float ly1x = Lerp(RandVecs2D[hash0 | 1], RandVecs2D[hash1 | 1], xs);
		xr += Lerp(lx0x, lx1x, ys) * warpAmp;
		yr += Lerp(ly0x, ly1x, ys) * warpAmp;
	}

	private void SingleDomainWarpBasicGrid(int seed, float warpAmp, float frequency, double x, double y, double z, ref double xr, ref double yr, ref double zr)
	{
		double xf = x * (double)frequency;
		double yf = y * (double)frequency;
		double zf = z * (double)frequency;
		int x2 = FastFloor(xf);
		int y2 = FastFloor(yf);
		int z2 = FastFloor(zf);
		float xs = InterpHermite((float)(xf - (double)x2));
		float ys = InterpHermite((float)(yf - (double)y2));
		float zs = InterpHermite((float)(zf - (double)z2));
		x2 *= 501125321;
		y2 *= 1136930381;
		z2 *= 1720413743;
		int x3 = x2 + 501125321;
		int y3 = y2 + 1136930381;
		int z3 = z2 + 1720413743;
		int hash0 = Hash(seed, x2, y2, z2) & 0x3FC;
		int hash1 = Hash(seed, x3, y2, z2) & 0x3FC;
		float lx0x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
		float ly0x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
		float lz0x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);
		hash0 = Hash(seed, x2, y3, z2) & 0x3FC;
		hash1 = Hash(seed, x3, y3, z2) & 0x3FC;
		float lx1x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
		float ly1x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
		float lz1x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);
		float lx0y = Lerp(lx0x, lx1x, ys);
		float ly0y = Lerp(ly0x, ly1x, ys);
		float lz0y = Lerp(lz0x, lz1x, ys);
		hash0 = Hash(seed, x2, y2, z3) & 0x3FC;
		hash1 = Hash(seed, x3, y2, z3) & 0x3FC;
		lx0x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
		ly0x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
		lz0x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);
		hash0 = Hash(seed, x2, y3, z3) & 0x3FC;
		hash1 = Hash(seed, x3, y3, z3) & 0x3FC;
		lx1x = Lerp(RandVecs3D[hash0], RandVecs3D[hash1], xs);
		ly1x = Lerp(RandVecs3D[hash0 | 1], RandVecs3D[hash1 | 1], xs);
		lz1x = Lerp(RandVecs3D[hash0 | 2], RandVecs3D[hash1 | 2], xs);
		xr += Lerp(lx0y, Lerp(lx0x, lx1x, ys), zs) * warpAmp;
		yr += Lerp(ly0y, Lerp(ly0x, ly1x, ys), zs) * warpAmp;
		zr += Lerp(lz0y, Lerp(lz0x, lz1x, ys), zs) * warpAmp;
	}

	private void SingleDomainWarpSimplexGradient(int seed, float warpAmp, float frequency, double x, double y, ref double xr, ref double yr, bool outGradOnly)
	{
		x *= (double)frequency;
		y *= (double)frequency;
		int i = FastFloor(x);
		int j = FastFloor(y);
		float xi = (float)(x - (double)i);
		float yi = (float)(y - (double)j);
		float t = (xi + yi) * 0.21132487f;
		float x2 = xi - t;
		float y2 = yi - t;
		i *= 501125321;
		j *= 1136930381;
		float vy;
		float vx = (vy = 0f);
		float a = 0.5f - x2 * x2 - y2 * y2;
		if (a > 0f)
		{
			float aaaa = a * a * (a * a);
			float xo;
			float yo;
			if (outGradOnly)
			{
				GradCoordOut(seed, i, j, out xo, out yo);
			}
			else
			{
				GradCoordDual(seed, i, j, x2, y2, out xo, out yo);
			}
			vx += aaaa * xo;
			vy += aaaa * yo;
		}
		float c = 3.1547005f * t + (-0.6666666f + a);
		if (c > 0f)
		{
			float x3 = x2 + -0.57735026f;
			float y3 = y2 + -0.57735026f;
			float cccc = c * c * (c * c);
			float xo2;
			float yo2;
			if (outGradOnly)
			{
				GradCoordOut(seed, i + 501125321, j + 1136930381, out xo2, out yo2);
			}
			else
			{
				GradCoordDual(seed, i + 501125321, j + 1136930381, x3, y3, out xo2, out yo2);
			}
			vx += cccc * xo2;
			vy += cccc * yo2;
		}
		if (y2 > x2)
		{
			float x4 = x2 + 0.21132487f;
			float y4 = y2 + -0.7886751f;
			float b = 0.5f - x4 * x4 - y4 * y4;
			if (b > 0f)
			{
				float bbbb = b * b * (b * b);
				float xo3;
				float yo3;
				if (outGradOnly)
				{
					GradCoordOut(seed, i, j + 1136930381, out xo3, out yo3);
				}
				else
				{
					GradCoordDual(seed, i, j + 1136930381, x4, y4, out xo3, out yo3);
				}
				vx += bbbb * xo3;
				vy += bbbb * yo3;
			}
		}
		else
		{
			float x5 = x2 + -0.7886751f;
			float y5 = y2 + 0.21132487f;
			float b2 = 0.5f - x5 * x5 - y5 * y5;
			if (b2 > 0f)
			{
				float bbbb2 = b2 * b2 * (b2 * b2);
				float xo4;
				float yo4;
				if (outGradOnly)
				{
					GradCoordOut(seed, i + 501125321, j, out xo4, out yo4);
				}
				else
				{
					GradCoordDual(seed, i + 501125321, j, x5, y5, out xo4, out yo4);
				}
				vx += bbbb2 * xo4;
				vy += bbbb2 * yo4;
			}
		}
		xr += vx * warpAmp;
		yr += vy * warpAmp;
	}

	private void SingleDomainWarpOpenSimplex2Gradient(int seed, float warpAmp, float frequency, double x, double y, double z, ref double xr, ref double yr, ref double zr, bool outGradOnly)
	{
		x *= (double)frequency;
		y *= (double)frequency;
		z *= (double)frequency;
		int i = FastRound(x);
		int j = FastRound(y);
		int k = FastRound(z);
		float x2 = (float)x - (float)i;
		float y2 = (float)y - (float)j;
		float z2 = (float)z - (float)k;
		int xNSign = (int)(0f - x2 - 1f) | 1;
		int yNSign = (int)(0f - y2 - 1f) | 1;
		int zNSign = (int)(0f - z2 - 1f) | 1;
		float ax0 = (float)xNSign * (0f - x2);
		float ay0 = (float)yNSign * (0f - y2);
		float az0 = (float)zNSign * (0f - z2);
		i *= 501125321;
		j *= 1136930381;
		k *= 1720413743;
		float vy;
		float vz;
		float vx = (vy = (vz = 0f));
		float a = 0.6f - x2 * x2 - (y2 * y2 + z2 * z2);
		int l = 0;
		while (true)
		{
			if (a > 0f)
			{
				float aaaa = a * a * (a * a);
				float xo;
				float yo;
				float zo;
				if (outGradOnly)
				{
					GradCoordOut(seed, i, j, k, out xo, out yo, out zo);
				}
				else
				{
					GradCoordDual(seed, i, j, k, x2, y2, z2, out xo, out yo, out zo);
				}
				vx += aaaa * xo;
				vy += aaaa * yo;
				vz += aaaa * zo;
			}
			float b = a;
			int i2 = i;
			int j2 = j;
			int k2 = k;
			float x3 = x2;
			float y3 = y2;
			float z3 = z2;
			if (ax0 >= ay0 && ax0 >= az0)
			{
				x3 += (float)xNSign;
				b = b + ax0 + ax0;
				i2 -= xNSign * 501125321;
			}
			else if (ay0 > ax0 && ay0 >= az0)
			{
				y3 += (float)yNSign;
				b = b + ay0 + ay0;
				j2 -= yNSign * 1136930381;
			}
			else
			{
				z3 += (float)zNSign;
				b = b + az0 + az0;
				k2 -= zNSign * 1720413743;
			}
			if (b > 1f)
			{
				b -= 1f;
				float bbbb = b * b * (b * b);
				float xo2;
				float yo2;
				float zo2;
				if (outGradOnly)
				{
					GradCoordOut(seed, i2, j2, k2, out xo2, out yo2, out zo2);
				}
				else
				{
					GradCoordDual(seed, i2, j2, k2, x3, y3, z3, out xo2, out yo2, out zo2);
				}
				vx += bbbb * xo2;
				vy += bbbb * yo2;
				vz += bbbb * zo2;
			}
			if (l == 1)
			{
				break;
			}
			ax0 = 0.5f - ax0;
			ay0 = 0.5f - ay0;
			az0 = 0.5f - az0;
			x2 = (float)xNSign * ax0;
			y2 = (float)yNSign * ay0;
			z2 = (float)zNSign * az0;
			a += 0.75f - ax0 - (ay0 + az0);
			i += (xNSign >> 1) & 0x1DDE90C9;
			j += (yNSign >> 1) & 0x43C42E4D;
			k += (zNSign >> 1) & 0x668B6E2F;
			xNSign = -xNSign;
			yNSign = -yNSign;
			zNSign = -zNSign;
			seed += 1293373;
			l++;
		}
		xr += vx * warpAmp;
		yr += vy * warpAmp;
		zr += vz * warpAmp;
	}
}
