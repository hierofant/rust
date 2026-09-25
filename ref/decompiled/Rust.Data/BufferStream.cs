using System;
using System.Runtime.CompilerServices;
using Facepunch;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

public sealed class BufferStream : IDisposable, Pool.IPooled
{
	public static class Shared
	{
		public static int StartingCapacity = 64;

		public static int MaximumCapacity = 536870912;

		public static int MaximumPooledSize = 67108864;

		public static readonly ArrayPool<byte> ArrayPool = new ArrayPool<byte>(MaximumPooledSize);
	}

	public readonly struct RepeatedElementLimitScope : IDisposable
	{
		private readonly BufferStream _stream;

		private readonly int _previousLimit;

		internal RepeatedElementLimitScope(BufferStream stream, int previousLimit)
		{
			_stream = stream;
			_previousLimit = previousLimit;
		}

		public void Dispose()
		{
			_stream._remainingRepeatedElements = _previousLimit;
		}
	}

	public readonly struct FieldOperationLimitScope : IDisposable
	{
		private readonly BufferStream _stream;

		private readonly bool _appliesLimit;

		private readonly string _previousContext;

		private readonly bool _previousRejectNonFiniteFloatingPointValues;

		internal FieldOperationLimitScope(BufferStream stream, bool appliesLimit, string previousContext, bool previousRejectNonFiniteFloatingPointValues)
		{
			_stream = stream;
			_appliesLimit = appliesLimit;
			_previousContext = previousContext;
			_previousRejectNonFiniteFloatingPointValues = previousRejectNonFiniteFloatingPointValues;
		}

		public void Dispose()
		{
			_stream._fieldOperationContext = _previousContext;
			_stream._rejectNonFiniteFloatingPointValues = _previousRejectNonFiniteFloatingPointValues;
			if (_appliesLimit)
			{
				_stream._fieldOperationLimit = -1;
				_stream._remainingFieldOperations = -1;
				_stream._validateFieldOrder = false;
			}
		}
	}

	public readonly struct FieldOrderValidationScope : IDisposable
	{
		private readonly BufferStream _stream;

		private readonly bool _previousValue;

		private readonly bool _suspended;

		internal FieldOrderValidationScope(BufferStream stream, bool previousValue, bool suspended)
		{
			_stream = stream;
			_previousValue = previousValue;
			_suspended = suspended;
		}

		public void Dispose()
		{
			if (_suspended)
			{
				_stream._validateFieldOrder = _previousValue;
			}
		}
	}

	public readonly struct FieldOperationLimitSuspensionScope : IDisposable
	{
		private readonly BufferStream _stream;

		private readonly int _previousRemainingOperations;

		private readonly bool _suspended;

		internal FieldOperationLimitSuspensionScope(BufferStream stream, int previousRemainingOperations, bool suspended)
		{
			_stream = stream;
			_previousRemainingOperations = previousRemainingOperations;
			_suspended = suspended;
		}

		public void Dispose()
		{
			if (_suspended)
			{
				_stream._remainingFieldOperations = _previousRemainingOperations;
			}
		}
	}

	public readonly ref struct RangeHandle
	{
		private readonly BufferStream _stream;

		private readonly int _offset;

		private readonly int _length;

		public RangeHandle(BufferStream stream, int offset, int length)
		{
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length");
			}
			_stream = stream ?? throw new ArgumentNullException("stream");
			_offset = offset;
			_length = length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<byte> GetSpan()
		{
			return new Span<byte>(_stream._buffer, _offset, _length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ArraySegment<byte> GetSegment()
		{
			return new ArraySegment<byte>(_stream._buffer, _offset, _length);
		}
	}

	public const int DefaultFieldOperationLimit = 4096;

	private bool _isBufferOwned;

	private byte[] _buffer;

	private int _length;

	private int _position;

	private int _fieldOperationLimit = -1;

	private int _remainingFieldOperations = -1;

	private string _fieldOperationContext;

	private bool _validateFieldOrder;

	private bool _rejectNonFiniteFloatingPointValues;

	private int _remainingRepeatedElements = -1;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _length;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (_position > value)
			{
				throw new InvalidOperationException("Cannot shrink buffer below current position!");
			}
			int num = value - _length;
			if (num > 0)
			{
				EnsureCapacity(num);
			}
			_length = value;
		}
	}

	public int Position
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _position;
		}
		set
		{
			if (value < 0 || value > _length)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			_position = value;
		}
	}

	public BufferStream Initialize()
	{
		_isBufferOwned = true;
		_buffer = null;
		_length = 0;
		_position = 0;
		_fieldOperationLimit = -1;
		_remainingFieldOperations = -1;
		_fieldOperationContext = null;
		_validateFieldOrder = false;
		_rejectNonFiniteFloatingPointValues = false;
		_remainingRepeatedElements = -1;
		return this;
	}

	public BufferStream Initialize(Span<byte> buffer)
	{
		_isBufferOwned = true;
		_buffer = null;
		_length = buffer.Length;
		_position = 0;
		_fieldOperationLimit = -1;
		_remainingFieldOperations = -1;
		_fieldOperationContext = null;
		_validateFieldOrder = false;
		_rejectNonFiniteFloatingPointValues = false;
		_remainingRepeatedElements = -1;
		EnsureCapacity(buffer.Length);
		buffer.CopyTo(_buffer);
		return this;
	}

	public BufferStream Initialize(byte[] buffer, int length = -1)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (length > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("length");
		}
		_isBufferOwned = false;
		_buffer = buffer;
		_length = ((length < 0) ? buffer.Length : length);
		_position = 0;
		_fieldOperationLimit = -1;
		_remainingFieldOperations = -1;
		_fieldOperationContext = null;
		_validateFieldOrder = false;
		_rejectNonFiniteFloatingPointValues = false;
		_remainingRepeatedElements = -1;
		return this;
	}

	public FieldOperationLimitScope BeginFieldOperationLimit(int maxOperations, string context)
	{
		if (maxOperations < 0)
		{
			throw new ArgumentOutOfRangeException("maxOperations");
		}
		bool flag = _remainingFieldOperations < 0;
		if (flag)
		{
			_fieldOperationLimit = maxOperations;
			_remainingFieldOperations = maxOperations;
			_validateFieldOrder = true;
		}
		string fieldOperationContext = _fieldOperationContext;
		bool rejectNonFiniteFloatingPointValues = _rejectNonFiniteFloatingPointValues;
		_fieldOperationContext = context;
		_rejectNonFiniteFloatingPointValues = true;
		return new FieldOperationLimitScope(this, flag, fieldOperationContext, rejectNonFiniteFloatingPointValues);
	}

	public FieldOrderValidationScope SuspendFieldOrderValidation()
	{
		return SuspendFieldOrderValidation(suspend: true);
	}

	public FieldOrderValidationScope SuspendFieldOrderValidation(bool suspend)
	{
		bool validateFieldOrder = _validateFieldOrder;
		if (suspend)
		{
			_validateFieldOrder = false;
		}
		return new FieldOrderValidationScope(this, validateFieldOrder, suspend);
	}

	public FieldOperationLimitSuspensionScope SuspendFieldOperationLimit()
	{
		return SuspendFieldOperationLimit(suspend: true);
	}

	public FieldOperationLimitSuspensionScope SuspendFieldOperationLimit(bool suspend)
	{
		int remainingFieldOperations = _remainingFieldOperations;
		if (suspend)
		{
			_remainingFieldOperations = -1;
		}
		return new FieldOperationLimitSuspensionScope(this, remainingFieldOperations, suspend);
	}

	public RepeatedElementLimitScope BeginRepeatedElementLimit(int maxElements)
	{
		int remainingRepeatedElements = _remainingRepeatedElements;
		_remainingRepeatedElements = maxElements;
		return new RepeatedElementLimitScope(this, remainingRepeatedElements);
	}

	public void Dispose()
	{
		if (_isBufferOwned && _buffer != null)
		{
			ReturnBuffer(_buffer);
		}
		_buffer = null;
		BufferStream obj = this;
		Pool.Free(ref obj);
	}

	void Pool.IPooled.EnterPool()
	{
		if (_isBufferOwned && _buffer != null)
		{
			ReturnBuffer(_buffer);
		}
		_buffer = null;
		_fieldOperationLimit = -1;
		_remainingFieldOperations = -1;
		_fieldOperationContext = null;
		_validateFieldOrder = false;
		_rejectNonFiniteFloatingPointValues = false;
		_remainingRepeatedElements = -1;
	}

	void Pool.IPooled.LeavePool()
	{
	}

	public void Clear()
	{
		_length = 0;
		_position = 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ConsumeRepeatedElement()
	{
		if (_remainingRepeatedElements >= 0)
		{
			if (_remainingRepeatedElements == 0)
			{
				throw new ProtocolBufferException("Repeated element budget exceeded");
			}
			_remainingRepeatedElements--;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ConsumeFieldOperation(string context)
	{
		if (_remainingFieldOperations >= 0)
		{
			_fieldOperationContext = context;
			if (_remainingFieldOperations == 0)
			{
				throw new ProtocolBufferException($"Field operation budget of {_fieldOperationLimit} exceeded while reading {_fieldOperationContext}");
			}
			_remainingFieldOperations--;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ValidateFieldOrder(ref uint lastFieldId, uint fieldId, bool fieldIsRepeated, string context)
	{
		if (_validateFieldOrder)
		{
			if (fieldId < lastFieldId || (fieldId == lastFieldId && !fieldIsRepeated))
			{
				throw new ProtocolBufferException($"Invalid field order in {context}: previous field {lastFieldId}, received field {fieldId}");
			}
			lastFieldId = fieldId;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ValidateFiniteValue(float value)
	{
		if (_rejectNonFiniteFloatingPointValues && (float.IsNaN(value) || float.IsInfinity(value)))
		{
			throw new ProtocolBufferException("Invalid float value");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void ValidateFiniteValue(double value)
	{
		if (_rejectNonFiniteFloatingPointValues && (double.IsNaN(value) || double.IsInfinity(value)))
		{
			throw new ProtocolBufferException("Invalid double value");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int ReadByte()
	{
		if (_position >= _length)
		{
			return -1;
		}
		return _buffer[_position++];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void WriteByte(byte b)
	{
		EnsureCapacity(1);
		_buffer[_position++] = b;
		_length = Math.Max(_length, _position);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Read<T>() where T : unmanaged
	{
		int num = Unsafe.SizeOf<T>();
		if (_length - _position < num)
		{
			ThrowReadOutOfBounds();
		}
		ref T reference = ref Unsafe.As<byte, T>(ref _buffer[_position]);
		_position += num;
		return reference;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Peek<T>() where T : unmanaged
	{
		int num = Unsafe.SizeOf<T>();
		if (_length - _position < num)
		{
			ThrowReadOutOfBounds();
		}
		return Unsafe.As<byte, T>(ref _buffer[_position]);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private void ThrowReadOutOfBounds()
	{
		throw new InvalidOperationException("Attempted to read past the end of the BufferStream");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Write<T>(T value) where T : unmanaged
	{
		int num = Unsafe.SizeOf<T>();
		EnsureCapacity(num);
		Unsafe.As<byte, T>(ref _buffer[_position]) = value;
		_position += num;
		_length = Math.Max(_length, _position);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public RangeHandle GetRange(int count)
	{
		EnsureCapacity(count);
		RangeHandle result = new RangeHandle(this, _position, count);
		_position += count;
		_length = Math.Max(_length, _position);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Skip(int count)
	{
		RequireRemaining(count);
		_position += count;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RequireRemaining(int count)
	{
		if (count < 0 || count > _length - _position)
		{
			ThrowReadOutOfBounds();
		}
	}

	public ArraySegment<byte> GetBuffer()
	{
		if (_length == 0)
		{
			return new ArraySegment<byte>(Array.Empty<byte>(), 0, 0);
		}
		return new ArraySegment<byte>(_buffer, 0, _length);
	}

	private void EnsureCapacity(int spaceRequired)
	{
		if (spaceRequired < 0)
		{
			throw new ArgumentOutOfRangeException("spaceRequired");
		}
		if (_buffer == null)
		{
			if (!_isBufferOwned)
			{
				throw new InvalidOperationException("Cannot allocate for BufferStream that doesn't own the buffer (did you forget to call Initialize?)");
			}
			int num = ((spaceRequired <= Shared.StartingCapacity) ? Shared.StartingCapacity : spaceRequired);
			int num2 = Mathf.NextPowerOfTwo(num);
			if (num2 > Shared.MaximumCapacity)
			{
				throw new Exception($"Preventing BufferStream buffer from growing too large (requiredLength={num})");
			}
			_buffer = RentBuffer(num2);
		}
		else if (_buffer.Length - _position < spaceRequired)
		{
			int num3 = _position + spaceRequired;
			int num4 = Mathf.NextPowerOfTwo(Math.Max(num3, _buffer.Length));
			if (!_isBufferOwned)
			{
				throw new InvalidOperationException($"Cannot grow buffer for BufferStream that doesn't own the buffer (requiredLength={num3})");
			}
			if (num4 > Shared.MaximumCapacity)
			{
				throw new Exception($"Preventing BufferStream buffer from growing too large (requiredLength={num3})");
			}
			byte[] array = RentBuffer(num4);
			Buffer.BlockCopy(_buffer, 0, array, 0, _length);
			ReturnBuffer(_buffer);
			_buffer = array;
		}
	}

	private static byte[] RentBuffer(int minSize)
	{
		if (minSize > Shared.MaximumPooledSize)
		{
			return new byte[minSize];
		}
		return Shared.ArrayPool.Rent(minSize);
	}

	private static void ReturnBuffer(byte[] buffer)
	{
		if (buffer != null && buffer.Length <= Shared.MaximumPooledSize)
		{
			Shared.ArrayPool.Return(buffer);
		}
	}
}
