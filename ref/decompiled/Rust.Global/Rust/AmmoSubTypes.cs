using System;

namespace Rust;

[Flags]
public enum AmmoSubTypes
{
	STANDARD = 1,
	HV = 2,
	INCENDIARY = 4,
	EXPLOSIVE = 8,
	SLUG = 0x10,
	SMOKE = 0x20,
	BUCKSHOT = 0x40,
	RADIATION = 0x80,
	SCATTER = 0x100,
	INCAPACITATE = 0x200
}
