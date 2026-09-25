namespace Rust.Rendering.IndirectInstancing;

public interface IMotionToggle
{
	void MotionStart();

	void OnBeforeMaterialChange();

	void OnAfterMaterialChange();
}
