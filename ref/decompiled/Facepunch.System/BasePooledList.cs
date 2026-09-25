using System;
using System.Collections.Generic;
using Facepunch;

public class BasePooledList<T, SubclassT> : List<T>, IDisposable, Pool.IPooled where SubclassT : BasePooledList<T, SubclassT>, new()
{
	void IDisposable.Dispose()
	{
		SubclassT obj = (SubclassT)this;
		Pool.Free(ref obj);
	}

	void Pool.IPooled.EnterPool()
	{
		Clear();
	}

	void Pool.IPooled.LeavePool()
	{
	}
}
