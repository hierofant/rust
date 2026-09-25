using System;

namespace Rust;

[Flags]
public enum EraRestriction
{
	Default = 0,
	Vending = 1,
	Loot = 2,
	Craft = 4,
	Mission = 8,
	Recycle = 0x10,
	MetalDetector = 0x20,
	None = 0x40
}
