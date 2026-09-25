using System;

namespace Epic.OnlineServices.AntiCheatCommon;

public struct LogEventParamPairParamValue
{
	private IntPtr? m_ClientHandle;

	private Utf8String m_String;

	private uint? m_UInt32;

	private int? m_Int32;

	private ulong? m_UInt64;

	private long? m_Int64;

	private Vec3f? m_Vec3f;

	private Quat? m_Quat;

	private float? m_Float;

	private AntiCheatCommonEventParamType m_ParamValueType;

	public IntPtr? ClientHandle
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.ClientHandle)
			{
				return m_ClientHandle;
			}
			return null;
		}
		set
		{
			m_ClientHandle = value;
			m_ParamValueType = AntiCheatCommonEventParamType.ClientHandle;
		}
	}

	public Utf8String String
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.String)
			{
				return m_String;
			}
			return null;
		}
		set
		{
			m_String = value;
			m_ParamValueType = AntiCheatCommonEventParamType.String;
		}
	}

	public uint? UInt32
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.UInt32)
			{
				return m_UInt32;
			}
			return null;
		}
		set
		{
			m_UInt32 = value;
			m_ParamValueType = AntiCheatCommonEventParamType.UInt32;
		}
	}

	public int? Int32
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.Int32)
			{
				return m_Int32;
			}
			return null;
		}
		set
		{
			m_Int32 = value;
			m_ParamValueType = AntiCheatCommonEventParamType.Int32;
		}
	}

	public ulong? UInt64
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.UInt64)
			{
				return m_UInt64;
			}
			return null;
		}
		set
		{
			m_UInt64 = value;
			m_ParamValueType = AntiCheatCommonEventParamType.UInt64;
		}
	}

	public long? Int64
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.Int64)
			{
				return m_Int64;
			}
			return null;
		}
		set
		{
			m_Int64 = value;
			m_ParamValueType = AntiCheatCommonEventParamType.Int64;
		}
	}

	public Vec3f? Vec3f
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.Vector3f)
			{
				return m_Vec3f;
			}
			return null;
		}
		set
		{
			m_Vec3f = value;
			m_ParamValueType = AntiCheatCommonEventParamType.Vector3f;
		}
	}

	public Quat? Quat
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.Quat)
			{
				return m_Quat;
			}
			return null;
		}
		set
		{
			m_Quat = value;
			m_ParamValueType = AntiCheatCommonEventParamType.Quat;
		}
	}

	public float? Float
	{
		get
		{
			if (m_ParamValueType == AntiCheatCommonEventParamType.Float)
			{
				return m_Float;
			}
			return null;
		}
		set
		{
			m_Float = value;
			m_ParamValueType = AntiCheatCommonEventParamType.Float;
		}
	}

	public AntiCheatCommonEventParamType ParamValueType => m_ParamValueType;

	public static implicit operator LogEventParamPairParamValue(IntPtr? value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.ClientHandle = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(Utf8String value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.String = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(string value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.String = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(uint? value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.UInt32 = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(int? value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.Int32 = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(ulong? value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.UInt64 = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(long? value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.Int64 = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(Vec3f? value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.Vec3f = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(Quat? value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.Quat = value;
		return result;
	}

	public static implicit operator LogEventParamPairParamValue(float? value)
	{
		LogEventParamPairParamValue result = default(LogEventParamPairParamValue);
		result.Float = value;
		return result;
	}
}
