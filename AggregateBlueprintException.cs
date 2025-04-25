using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public sealed class AggregateBlueprintException : BlueprintException
{
	public ReadOnlyCollection<BlueprintException> InnerExceptions { get; }

	public AggregateBlueprintException()
		: base("No BlueprintException occured.")
	{
	}

	public AggregateBlueprintException(BlueprintException innerException)
		: base("A BlueprintException occured.", innerException)
	{
		InnerExceptions = new ReadOnlyCollection<BlueprintException>(new BlueprintException[1] { innerException });
	}

	public AggregateBlueprintException(params BlueprintException[] innerExceptions)
		: base("One or more BlueprintExceptions occured.")
	{
		InnerExceptions = new ReadOnlyCollection<BlueprintException>(innerExceptions);
	}

	public AggregateBlueprintException(IEnumerable<BlueprintException> innerExceptions)
		: base("One or more BlueprintExceptions occured.")
	{
		InnerExceptions = new ReadOnlyCollection<BlueprintException>(innerExceptions.ToArray());
	}
}
