public interface IAiInputProvider
{
	void OnAdd(BaseVehicle vehicle);

	void OnTick(BaseVehicle vehicle, float delta);

	void OnTick(BaseVehicle vehicle, float delta, ref float steering, ref float gasPedal);

	void OnRemove(BaseVehicle vehicle);
}
