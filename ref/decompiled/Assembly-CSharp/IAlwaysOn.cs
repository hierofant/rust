public interface IAlwaysOn
{
	void SetAlwaysOn(bool flag);

	bool IsAlwaysOn()
	{
		return false;
	}

	void AlwaysOnToggled(bool flag);
}
