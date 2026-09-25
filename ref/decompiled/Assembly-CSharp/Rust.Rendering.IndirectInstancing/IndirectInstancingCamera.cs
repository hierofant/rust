using UnityEngine;

namespace Rust.Rendering.IndirectInstancing;

[DefaultExecutionOrder(-1250)]
public class IndirectInstancingCamera : SingletonComponent<IndirectInstancingCamera>
{
	public Shader[] supportedShaders;
}
