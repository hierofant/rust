using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Facepunch;
using Newtonsoft.Json;
using UnityEngine;

public class TextTable : Pool.IPooled, IDisposable
{
	[StructLayout(LayoutKind.Explicit)]
	private struct RowValueUnion
	{
		[FieldOffset(0)]
		public bool Bool;

		[FieldOffset(0)]
		public int Int;

		[FieldOffset(0)]
		public uint UInt;

		[FieldOffset(0)]
		public long Long;

		[FieldOffset(0)]
		public ulong ULong;

		[FieldOffset(0)]
		public float Float;

		[FieldOffset(0)]
		public double Double;

		[FieldOffset(0)]
		public Vector3 Vec3;

		[FieldOffset(0)]
		public string String;
	}

	private enum ValueType
	{
		Bool,
		Int,
		UInt,
		Long,
		ULong,
		Float,
		Double,
		Vec3,
		String,
		NextRow
	}

	private struct RowValue
	{
		public RowValueUnion Value;

		public ValueType ValueType;

		public void WriteTo(StringBuilder builder)
		{
			switch (ValueType)
			{
			case ValueType.Bool:
				builder.Append(Value.Bool);
				break;
			case ValueType.Int:
			{
				Span<char> destination7 = stackalloc char[32];
				Value.Int.TryFormat(destination7, out var charsWritten7);
				builder.Append(destination7.Slice(0, charsWritten7));
				break;
			}
			case ValueType.UInt:
			{
				Span<char> destination6 = stackalloc char[32];
				Value.UInt.TryFormat(destination6, out var charsWritten6);
				builder.Append(destination6.Slice(0, charsWritten6));
				break;
			}
			case ValueType.Long:
			{
				Span<char> destination5 = stackalloc char[32];
				Value.Long.TryFormat(destination5, out var charsWritten5);
				builder.Append(destination5.Slice(0, charsWritten5));
				break;
			}
			case ValueType.ULong:
			{
				Span<char> destination4 = stackalloc char[32];
				Value.ULong.TryFormat(destination4, out var charsWritten4);
				builder.Append(destination4.Slice(0, charsWritten4));
				break;
			}
			case ValueType.Float:
			{
				Span<char> destination3 = stackalloc char[32];
				Value.Float.TryFormat(destination3, out var charsWritten3);
				builder.Append(destination3.Slice(0, charsWritten3));
				break;
			}
			case ValueType.Double:
			{
				Span<char> destination2 = stackalloc char[32];
				Value.Double.TryFormat(destination2, out var charsWritten2);
				builder.Append(destination2.Slice(0, charsWritten2));
				break;
			}
			case ValueType.Vec3:
			{
				Span<char> destination = stackalloc char[32];
				builder.Append('(');
				NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;
				Value.Vec3.x.TryFormat(destination, out var charsWritten, "F2", numberFormat);
				builder.Append(destination.Slice(0, charsWritten));
				builder.Append(", ");
				Value.Vec3.y.TryFormat(destination, out charsWritten, "F2", numberFormat);
				builder.Append(destination.Slice(0, charsWritten));
				builder.Append(", ");
				Value.Vec3.z.TryFormat(destination, out charsWritten, "F2", numberFormat);
				builder.Append(destination.Slice(0, charsWritten));
				builder.Append(')');
				break;
			}
			case ValueType.String:
				builder.Append(Value.String);
				break;
			}
		}

		public void WriteTo(JsonTextWriter writer, TextWriter textWriter, bool stringify)
		{
			if (stringify)
			{
				switch (ValueType)
				{
				case ValueType.Bool:
					writer.WriteRaw("\"");
					writer.WriteRawValue(Value.Bool ? "True" : "False");
					writer.WriteRaw("\"");
					break;
				case ValueType.Int:
					writer.WriteRaw("\"");
					writer.WriteValue(Value.Int);
					writer.WriteRaw("\"");
					break;
				case ValueType.UInt:
					writer.WriteRaw("\"");
					writer.WriteValue(Value.UInt);
					writer.WriteRaw("\"");
					break;
				case ValueType.Long:
					writer.WriteRaw("\"");
					writer.WriteValue(Value.Long);
					writer.WriteRaw("\"");
					break;
				case ValueType.ULong:
					writer.WriteRaw("\"");
					writer.WriteValue(Value.ULong);
					writer.WriteRaw("\"");
					break;
				case ValueType.Float:
				{
					Span<char> destination2 = stackalloc char[32];
					Value.Float.TryFormat(destination2, out var charsWritten2);
					writer.WriteRaw("\"");
					for (int l = 0; l < charsWritten2; l++)
					{
						textWriter.Write(destination2[l]);
					}
					writer.WriteRaw("\"");
					writer.WriteRawValue(null);
					break;
				}
				case ValueType.Double:
				{
					Span<char> destination3 = stackalloc char[32];
					Value.Double.TryFormat(destination3, out var charsWritten3);
					writer.WriteRaw("\"");
					for (int m = 0; m < charsWritten3; m++)
					{
						textWriter.Write(destination3[m]);
					}
					writer.WriteRaw("\"");
					writer.WriteRawValue(null);
					break;
				}
				case ValueType.Vec3:
				{
					writer.WriteRaw("\"(");
					Span<char> destination = stackalloc char[32];
					NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;
					Value.Vec3.x.TryFormat(destination, out var charsWritten, "F2", numberFormat);
					for (int i = 0; i < charsWritten; i++)
					{
						textWriter.Write(destination[i]);
					}
					writer.WriteRaw(", ");
					Value.Vec3.y.TryFormat(destination, out charsWritten, "F2", numberFormat);
					for (int j = 0; j < charsWritten; j++)
					{
						textWriter.Write(destination[j]);
					}
					writer.WriteRaw(", ");
					Value.Vec3.z.TryFormat(destination, out charsWritten, "F2", numberFormat);
					for (int k = 0; k < charsWritten; k++)
					{
						textWriter.Write(destination[k]);
					}
					writer.WriteRaw(")\"");
					writer.WriteRawValue(null);
					break;
				}
				case ValueType.String:
					writer.WriteValue(Value.String);
					break;
				}
				return;
			}
			switch (ValueType)
			{
			case ValueType.Bool:
				writer.WriteValue(Value.Bool);
				break;
			case ValueType.Int:
				writer.WriteValue(Value.Int);
				break;
			case ValueType.UInt:
				writer.WriteValue(Value.UInt);
				break;
			case ValueType.Long:
				writer.WriteValue(Value.Long);
				break;
			case ValueType.ULong:
				writer.WriteValue(Value.ULong);
				break;
			case ValueType.Float:
			{
				Span<char> destination5 = stackalloc char[32];
				Value.Float.TryFormat(destination5, out var charsWritten5);
				for (int num3 = 0; num3 < charsWritten5; num3++)
				{
					textWriter.Write(destination5[num3]);
				}
				writer.WriteRawValue(null);
				break;
			}
			case ValueType.Double:
			{
				Span<char> destination6 = stackalloc char[32];
				Value.Double.TryFormat(destination6, out var charsWritten6);
				for (int num4 = 0; num4 < charsWritten6; num4++)
				{
					textWriter.Write(destination6[num4]);
				}
				writer.WriteRawValue(null);
				break;
			}
			case ValueType.Vec3:
			{
				writer.WriteStartArray();
				Span<char> destination4 = stackalloc char[32];
				NumberFormatInfo numberFormat2 = CultureInfo.InvariantCulture.NumberFormat;
				Value.Vec3.x.TryFormat(destination4, out var charsWritten4, "F2", numberFormat2);
				for (int n = 0; n < charsWritten4; n++)
				{
					textWriter.Write(destination4[n]);
				}
				textWriter.Write(',');
				Value.Vec3.y.TryFormat(destination4, out charsWritten4, "F2", numberFormat2);
				for (int num = 0; num < charsWritten4; num++)
				{
					textWriter.Write(destination4[num]);
				}
				textWriter.Write(',');
				Value.Vec3.z.TryFormat(destination4, out charsWritten4, "F2", numberFormat2);
				for (int num2 = 0; num2 < charsWritten4; num2++)
				{
					textWriter.Write(destination4[num2]);
				}
				writer.WriteEndArray();
				break;
			}
			case ValueType.String:
				writer.WriteValue(Value.String);
				break;
			}
		}
	}

	private struct Column
	{
		public string title;

		public int width;

		public Column(string title)
		{
			this.title = title;
			width = title.Length;
		}
	}

	private static Encoding utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	private BufferList<RowValue> rowValues;

	private BufferList<Column> columns;

	private string text;

	private string jsonText;

	private bool wasPooled;

	public bool ShouldPadColumns;

	public TextTable()
		: this(shouldPadColumns: true)
	{
	}

	public TextTable(bool shouldPadColumns)
	{
		ShouldPadColumns = shouldPadColumns;
	}

	void Pool.IPooled.EnterPool()
	{
		Pool.FreeUnmanaged(ref columns);
		Pool.FreeUnmanaged(ref rowValues);
		ShouldPadColumns = true;
	}

	void Pool.IPooled.LeavePool()
	{
		columns = Pool.Get<BufferList<Column>>();
		rowValues = Pool.Get<BufferList<RowValue>>();
		wasPooled = true;
	}

	void IDisposable.Dispose()
	{
		if (wasPooled)
		{
			TextTable obj = this;
			Pool.Free(ref obj);
		}
	}

	public void Clear()
	{
		columns?.Clear();
		rowValues?.Clear();
		MarkDirty();
	}

	public void ResizeColumns(int count)
	{
		if (columns == null)
		{
			columns = new BufferList<Column>();
		}
		columns.Resize(count);
	}

	public void AddColumns(params string[] values)
	{
		ResizeColumns(values.Length);
		for (int i = 0; i < values.Length; i++)
		{
			columns.Add(new Column(values[i]));
		}
		MarkDirty();
	}

	public void AddColumn(string title)
	{
		if (columns == null)
		{
			columns = new BufferList<Column>();
		}
		columns.Add(new Column(title));
		MarkDirty();
	}

	public void ResizeRows(int count)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		rowValues.Resize(count * columns.Count);
	}

	public void AddRow(params string[] values)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		int num = Mathf.Min(columns.Count, values.Length);
		for (int i = 0; i < num; i++)
		{
			if (ShouldPadColumns)
			{
				columns.Buffer[i].width = Mathf.Max(columns[i].width, values[i].Length);
			}
			RowValue rowValue = default(RowValue);
			rowValue.Value = new RowValueUnion
			{
				String = values[i]
			};
			rowValue.ValueType = ValueType.String;
			RowValue element = rowValue;
			rowValues.Add(element);
		}
		if (num < columns.Count)
		{
			RowValue rowValue = default(RowValue);
			rowValue.ValueType = ValueType.NextRow;
			RowValue element2 = rowValue;
			rowValues.Add(element2);
		}
		MarkDirty();
	}

	public void AddValue(bool value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			Bool = value
		};
		rowValue.ValueType = ValueType.Bool;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			int val = (value ? 4 : 5);
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, val);
		}
		rowValues.Add(element);
	}

	public void AddValue(int value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			Int = value
		};
		rowValue.ValueType = ValueType.Int;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			int val = LengthOf(value);
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, val);
		}
		rowValues.Add(element);
	}

	public void AddValue(uint value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			UInt = value
		};
		rowValue.ValueType = ValueType.UInt;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			int val = LengthOf(value);
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, val);
		}
		rowValues.Add(element);
	}

	public void AddValue(long value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			Long = value
		};
		rowValue.ValueType = ValueType.Long;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			int val = LengthOf(value);
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, val);
		}
		rowValues.Add(element);
	}

	public void AddValue(ulong value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			ULong = value
		};
		rowValue.ValueType = ValueType.ULong;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			int val = LengthOf(value);
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, val);
		}
		rowValues.Add(element);
	}

	public void AddValue(float value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			Float = value
		};
		rowValue.ValueType = ValueType.Float;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			int val = LengthOf(value);
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, val);
		}
		rowValues.Add(element);
	}

	public void AddValue(double value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			Double = value
		};
		rowValue.ValueType = ValueType.Double;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			int val = LengthOf(value);
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, val);
		}
		rowValues.Add(element);
	}

	public void AddValue(Vector3 value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			Vec3 = value
		};
		rowValue.ValueType = ValueType.Vec3;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			int val = LengthOf(value);
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, val);
		}
		rowValues.Add(element);
	}

	public void AddValue(string value)
	{
		if (rowValues == null)
		{
			rowValues = new BufferList<RowValue>();
		}
		RowValue rowValue = default(RowValue);
		rowValue.Value = new RowValueUnion
		{
			String = value
		};
		rowValue.ValueType = ValueType.String;
		RowValue element = rowValue;
		if (ShouldPadColumns)
		{
			int num = rowValues.Count % columns.Count;
			ref Column reference = ref columns.Buffer[num];
			reference.width = Math.Max(reference.width, value.Length);
		}
		rowValues.Add(element);
	}

	public string ToJson(bool stringify = true)
	{
		if (jsonText == null)
		{
			if (columns == null || rowValues == null)
			{
				jsonText = "[]";
				return jsonText;
			}
			using MemoryStream memoryStream = new MemoryStream();
			using (StreamWriter textWriter = new StreamWriter(memoryStream, utf8NoBom, 1024, leaveOpen: true))
			{
				using JsonTextWriter jsonTextWriter = new JsonTextWriter(textWriter);
				jsonTextWriter.WriteStartArray();
				int num = 0;
				while (num < rowValues.Count)
				{
					jsonTextWriter.WriteStartObject();
					for (int i = 0; i < columns.Count; i++)
					{
						if (num >= rowValues.Count)
						{
							break;
						}
						RowValue rowValue = rowValues[num++];
						if (rowValue.ValueType == ValueType.NextRow)
						{
							break;
						}
						jsonTextWriter.WritePropertyName(columns[i].title);
						rowValue.WriteTo(jsonTextWriter, textWriter, stringify);
					}
					jsonTextWriter.WriteEndObject();
				}
				jsonTextWriter.WriteEndArray();
			}
			jsonText = Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
		}
		return jsonText;
	}

	public override string ToString()
	{
		if (text == null)
		{
			if (columns == null)
			{
				text = string.Empty;
				return text;
			}
			StringBuilder obj = Pool.Get<StringBuilder>();
			for (int i = 0; i < columns.Count; i++)
			{
				obj.Append(columns[i].title);
				int length = columns[i].title.Length;
				int num = columns[i].width + 1;
				for (int j = length; j < num; j++)
				{
					obj.Append(' ');
				}
			}
			obj.AppendLine();
			if (rowValues != null)
			{
				int num2 = 0;
				while (num2 < rowValues.Count)
				{
					for (int k = 0; k < columns.Count; k++)
					{
						if (num2 >= rowValues.Count)
						{
							break;
						}
						RowValue rowValue = rowValues[num2++];
						if (rowValue.ValueType == ValueType.NextRow)
						{
							break;
						}
						int length2 = obj.Length;
						rowValue.WriteTo(obj);
						int num3 = obj.Length - length2;
						int num4 = columns[k].width + 1;
						for (int l = num3; l < num4; l++)
						{
							obj.Append(' ');
						}
					}
					obj.AppendLine();
				}
			}
			text = obj.ToString();
			Pool.FreeUnmanaged(ref obj);
		}
		return text;
	}

	private void MarkDirty()
	{
		jsonText = null;
		text = null;
	}

	private static int LengthOf(int i)
	{
		return ((i < 0) ? 1 : 0) + LengthOf((uint)Math.Abs(i));
	}

	private static int LengthOf(uint u)
	{
		if (u < 100000)
		{
			if (u < 100)
			{
				return (u < 10) ? 1 : 2;
			}
			if (u < 10000)
			{
				return (u < 1000) ? 3 : 4;
			}
			return 5;
		}
		if (u < 100000000)
		{
			if (u < 10000000)
			{
				return (u < 1000000) ? 6 : 7;
			}
			return 8;
		}
		return (u < 1000000000) ? 9 : 10;
	}

	private static int LengthOf(long l)
	{
		return ((l < 0) ? 1 : 0) + LengthOf((ulong)Math.Abs(l));
	}

	private static int LengthOf(ulong ul)
	{
		if (ul < 10000000000L)
		{
			if (ul < 100000)
			{
				if (ul < 100)
				{
					return (ul < 10) ? 1 : 2;
				}
				if (ul < 10000)
				{
					return (ul < 1000) ? 3 : 4;
				}
				return 5;
			}
			if (ul < 100000000)
			{
				if (ul < 10000000)
				{
					return (ul < 1000000) ? 6 : 7;
				}
				return 8;
			}
			return (ul < 1000000000) ? 9 : 10;
		}
		if (ul < 1000000000000000L)
		{
			if (ul < 1000000000000L)
			{
				return (ul < 100000000000L) ? 11 : 12;
			}
			if (ul < 100000000000000L)
			{
				return (ul < 10000000000000L) ? 13 : 14;
			}
			return 15;
		}
		if (ul < 1000000000000000000L)
		{
			if (ul < 100000000000000000L)
			{
				return (ul < 10000000000000000L) ? 16 : 17;
			}
			return 18;
		}
		return (ul < 10000000000000000000uL) ? 19 : 20;
	}

	private static int LengthOf(float f)
	{
		Span<char> destination = stackalloc char[32];
		f.TryFormat(destination, out var charsWritten);
		return charsWritten;
	}

	private static int LengthOf(double d)
	{
		Span<char> destination = stackalloc char[32];
		d.TryFormat(destination, out var charsWritten);
		return charsWritten;
	}

	private static int LengthOf(Vector3 v)
	{
		int num = 6;
		Span<char> destination = stackalloc char[32];
		NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;
		v.x.TryFormat(destination, out var charsWritten, "F2", numberFormat);
		num += charsWritten;
		v.y.TryFormat(destination, out charsWritten, "F2", numberFormat);
		num += charsWritten;
		v.z.TryFormat(destination, out charsWritten, "F2", numberFormat);
		return num + charsWritten;
	}
}
