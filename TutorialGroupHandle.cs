using System.Collections.Generic;
using System.Linq;

public class TutorialGroupHandle : TutorialEntryHandle
{
	private TutorialStepHandle[] _Steps;

	public IReadOnlyCollection<TutorialStepHandle> Steps => (IReadOnlyCollection<TutorialStepHandle>)(object)_Steps;

	public TutorialGroupHandle(MetaTutorialGroup metaData)
		: base(metaData)
	{
		_Steps = metaData.Steps.Select((MetaTutorialStep step) => new TutorialStepHandle(step)).ToArray();
	}
}
