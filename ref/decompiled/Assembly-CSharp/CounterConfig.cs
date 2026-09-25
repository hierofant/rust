using System;
using Rust.UI;
using UnityEngine;

public class CounterConfig : IOConfig<PowerCounter>
{
	[SerializeField]
	private RustInput resetInput;

	[NonSerialized]
	public float resetTarget;
}
