public interface IExpiringResource
{
	string Name { get; }

	double LastUsed { get; }

	float ExpireAfter { get; }

	void Hook_OnExpire();
}
