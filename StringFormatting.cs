using System;
using System.Globalization;
using Unity.Mathematics;

public static class StringFormatting
{
	private const bool DEBUG = false;

	private static CultureInfo CurrentCulture => CultureInfo.DefaultThreadCurrentCulture;

	public static string FormatGeneralPercentage(float percentage0To1)
	{
		bool flag = false;
		return percentage0To1.ToString("P0", CurrentCulture);
	}

	public static string FormatShapeAmount(int amount)
	{
		bool flag = false;
		return FormatIntegerMax4Digits(amount);
	}

	public static string FormatGenericCount(uint amount)
	{
		bool flag = false;
		return FormatGenericCount((int)amount);
	}

	public static string FormatGenericCount(long amount)
	{
		bool flag = false;
		return FormatGenericCount((int)amount);
	}

	public static string FormatGenericCount(int amount)
	{
		bool flag = false;
		return FormatIntegerMax4Digits(amount);
	}

	public static string FormatBlueprintCurrency(BlueprintCurrency amount)
	{
		bool flag = false;
		return FormatIntegerMax4Digits((int)amount.TotalMain);
	}

	public static string FormatDimensions(int sizeX, int sizeY)
	{
		bool flag = false;
		return "unit.dimensions-2d".tr(("<x>", FormatIntegerRaw(sizeX)), ("<y>", FormatIntegerRaw(sizeY)));
	}

	public static string FormatDimensions(int sizeX, int sizeY, int sizeZ)
	{
		bool flag = false;
		return "unit.dimensions-3d".tr(("<x>", FormatIntegerRaw(sizeX)), ("<y>", FormatIntegerRaw(sizeY)), ("<z>", FormatIntegerRaw(sizeZ)));
	}

	public static string FormatBeltSpeed(int shapesPerMinute)
	{
		bool flag = false;
		return "unit.shapes-per-minute".tr(("<amount>", shapesPerMinute.ToString(CurrentCulture)));
	}

	public static string FormatShapeAmountFraction(int have, int goal)
	{
		bool flag = false;
		return "<b>" + FormatShapeAmount(have) + "</b> / " + FormatShapeAmount(goal);
	}

	public static string FormatShapeAmountThroughputPerMinute(float throughput)
	{
		bool flag = false;
		string formatted = FormatShapeAmountThroughputPerMinuteRaw(throughput);
		return "unit.shapes-per-minute".tr(("<amount>", formatted));
	}

	public static string FormatShapeAmountThroughputPerMinuteRaw(float throughput)
	{
		bool flag = false;
		return throughput.ToString("0.##", CurrentCulture);
	}

	public static string FormatResearchNodeTier(int tier)
	{
		bool flag = false;
		return "<b>" + FormatIntegerRaw(tier) + "</b>";
	}

	public static string FormatEfficiencyPercentage(float percentage0To1)
	{
		bool flag = false;
		return FormatGeneralPercentage(percentage0To1);
	}

	public static string FormatLiters(float amount)
	{
		bool flag = false;
		string liters = FormatIntegerMax4Digits((int)amount);
		return "unit.liters".tr(("<amount>", liters));
	}

	public static string FormatLitersFlowPerMinuteSigned(float amount)
	{
		bool flag = false;
		int roundedValue = (int)math.round(amount);
		if (roundedValue == 0)
		{
			return "-";
		}
		string formatted = FormatIntegerMax4Digits(math.abs(roundedValue));
		return ((amount < 0f) ? "-" : "+") + "unit.liters-per-minute".tr(("<amount>", formatted));
	}

	public static string FormatSecondsToPastTime(float secondsFractional)
	{
		bool flag = false;
		int seconds = (int)math.floor(secondsFractional);
		int minutes = seconds / 60;
		int hours = minutes / 60;
		int days = hours / 24;
		if (seconds < 60)
		{
			if (seconds == 1)
			{
				return "time.one-second-ago".tr();
			}
			return "time.x-seconds-ago".tr(("<amount>", FormatIntegerRaw(seconds)));
		}
		if (minutes < 60)
		{
			if (minutes == 1)
			{
				return "time.one-minute-ago".tr();
			}
			return "time.x-minutes-ago".tr(("<amount>", FormatIntegerRaw(minutes)));
		}
		if (hours < 24)
		{
			if (hours == 1)
			{
				return "time.one-hour-ago".tr();
			}
			return "time.x-hours-ago".tr(("<amount>", FormatIntegerRaw(hours)));
		}
		if (days == 1)
		{
			return "time.one-day-ago".tr();
		}
		return "time.x-days-ago".tr(("<amount>", FormatIntegerRaw(days)));
	}

	public static string FormatPastTime(DateTime pastTime)
	{
		bool flag = false;
		return FormatSecondsToPastTime((float)(DateTime.Now - pastTime).TotalSeconds);
	}

	public static string FormatDurationSeconds(float secondsFractional)
	{
		bool flag = false;
		int seconds = (int)math.floor(secondsFractional);
		if (seconds < 60)
		{
			return "time.seconds-short".tr(("<seconds>", FormatIntegerRaw(seconds)));
		}
		if (seconds < 3600)
		{
			return "time.minutes-seconds-short".tr(("<seconds>", FormatIntegerRaw(seconds % 60)), ("<minutes>", FormatIntegerRaw(seconds / 60)));
		}
		int hours = seconds / 3600;
		int minutes = seconds / 60 % 60;
		return "time.hours-minutes-short".tr(("<minutes>", FormatIntegerRaw(minutes)), ("<hours>", FormatIntegerRaw(hours)));
	}

	public static string FormatIntegerRaw(int value)
	{
		bool flag = false;
		return value.ToString("N0", CurrentCulture);
	}

	public static string FormatIntegerMax4Digits(int value)
	{
		bool flag = false;
		if (value < 9999)
		{
			return FormatIntegerRaw(value);
		}
		if (value < 99999)
		{
			string format = "N1";
			if (value % 1000 == 0)
			{
				format = "N0";
			}
			double thousands = (double)value / 1000.0;
			return "unit.thousands-short".tr(("<amount>", thousands.ToString(format, CurrentCulture)));
		}
		if (value < 999999)
		{
			double thousands2 = (double)value / 1000.0;
			return "unit.thousands-short".tr(("<amount>", thousands2.ToString("N0", CurrentCulture)));
		}
		double millions = (double)value / 1000000.0;
		return "unit.millions-short".tr(("<amount>", millions.ToString("N2", CurrentCulture)));
	}
}
