namespace Epic.OnlineServices.Lobby;

public struct AttributeDataValue
{
	private long? m_AsInt64;

	private double? m_AsDouble;

	private bool? m_AsBool;

	private Utf8String m_AsUtf8;

	private AttributeType m_ValueType;

	public long? AsInt64
	{
		get
		{
			if (m_ValueType == AttributeType.Int64)
			{
				return m_AsInt64;
			}
			return null;
		}
		set
		{
			m_AsInt64 = value;
			m_ValueType = AttributeType.Int64;
		}
	}

	public double? AsDouble
	{
		get
		{
			if (m_ValueType == AttributeType.Double)
			{
				return m_AsDouble;
			}
			return null;
		}
		set
		{
			m_AsDouble = value;
			m_ValueType = AttributeType.Double;
		}
	}

	public bool? AsBool
	{
		get
		{
			if (m_ValueType == AttributeType.Boolean)
			{
				return m_AsBool;
			}
			return null;
		}
		set
		{
			m_AsBool = value;
			m_ValueType = AttributeType.Boolean;
		}
	}

	public Utf8String AsUtf8
	{
		get
		{
			if (m_ValueType == AttributeType.String)
			{
				return m_AsUtf8;
			}
			return null;
		}
		set
		{
			m_AsUtf8 = value;
			m_ValueType = AttributeType.String;
		}
	}

	public AttributeType ValueType => m_ValueType;

	public static implicit operator AttributeDataValue(long? value)
	{
		AttributeDataValue result = default(AttributeDataValue);
		result.AsInt64 = value;
		return result;
	}

	public static implicit operator AttributeDataValue(double? value)
	{
		AttributeDataValue result = default(AttributeDataValue);
		result.AsDouble = value;
		return result;
	}

	public static implicit operator AttributeDataValue(bool? value)
	{
		AttributeDataValue result = default(AttributeDataValue);
		result.AsBool = value;
		return result;
	}

	public static implicit operator AttributeDataValue(Utf8String value)
	{
		AttributeDataValue result = default(AttributeDataValue);
		result.AsUtf8 = value;
		return result;
	}

	public static implicit operator AttributeDataValue(string value)
	{
		AttributeDataValue result = default(AttributeDataValue);
		result.AsUtf8 = value;
		return result;
	}
}
