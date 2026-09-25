using System;
using Rust;

public interface IEngineControllerUser : IEntity
{
	bool HasFlag(BaseEntity.Flags f);

	bool IsDead();

	void Invoke(Action action, float time);

	void CancelInvoke(Action action);

	void OnEngineStartFailed();

	bool MeetsEngineRequirements();
}
