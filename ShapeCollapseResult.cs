using System;

public class ShapeCollapseResult
{
	public ShapeCollapseResultEntry[] Entries;

	public string ResultDefinition;

	public bool ResultsInEmptyShape => ResultDefinition == null;

	public static void Sync(ISerializationVisitor visitor, ref ShapeCollapseResult target)
	{
		if (visitor.Version < 1001)
		{
			throw new Exception("Min savegame version is 1001 (is " + visitor.Version + ") for serializing collapse results");
		}
		if (visitor.Writing)
		{
			if (target == null || target.Entries.Length == 0)
			{
				visitor.WriteByte_1(0);
				return;
			}
			visitor.WriteByte_1((byte)target.Entries.Length);
			visitor.WriteString_4(target.ResultDefinition);
			ShapeCollapseResultEntry[] entries = target.Entries;
			for (int i = 0; i < entries.Length; i++)
			{
				ShapeCollapseResultEntry entry = entries[i];
				visitor.WriteString_4(entry.ShapeDefinition);
				visitor.WriteByte_1((byte)entry.FallDownLayers);
				visitor.WriteByte_1((byte)(entry.Vanish ? 1u : 0u));
			}
			return;
		}
		byte count = visitor.ReadByte_1();
		if (count == 0)
		{
			target = null;
			return;
		}
		ShapeCollapseResultEntry[] entries2 = new ShapeCollapseResultEntry[count];
		string resultDefinition = visitor.ReadString_4();
		for (int j = 0; j < count; j++)
		{
			string entryDefinition = visitor.ReadString_4();
			int entryFallDown = visitor.ReadByte_1();
			bool entryVanish = visitor.ReadByte_1() == 1;
			entries2[j] = new ShapeCollapseResultEntry
			{
				ShapeDefinition = entryDefinition,
				FallDownLayers = entryFallDown,
				Vanish = entryVanish
			};
		}
		target = new ShapeCollapseResult
		{
			Entries = entries2,
			ResultDefinition = resultDefinition
		};
	}
}
