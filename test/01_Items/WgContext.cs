using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

namespace _01_Items
{
	public class GrainInfo
	{
		public string MethodAssembly;
		public string MethodClass;
		public string MethodName;

		public string DataAssembly;
		public string DataType;
	}

	public class CallStackEntry
	{
		public Delegate Proc;
		public object Data;     // if null, inherits previous Data

		public object EffectiveData;	// for runtime only
	}

	public partial class WgContext
	{
		//
		protected Stack<CallStackEntry> CallStack = new Stack<CallStackEntry> ();
		protected CallStackEntry CurrentEntry;

		public void Run (WaitHandle ehStop)
		{
			while (!ehStop.WaitOne (0) && CallStack.Count > 0)
			{
				CurrentEntry = CallStack.Pop ();
				CurrentEntry.Proc.Method.Invoke (null, BindingFlags.Default, null, new[] { this, CurrentEntry.EffectiveData }, Thread.CurrentThread.CurrentCulture);
			}
		}

		//
		public void ProceedToGeneric (Delegate NextProc, object Data, Delegate FurtherProc, uint StartAt = 0)
		{
			if (FurtherProc != null)
			{
				CallStack.Push (new CallStackEntry
					{
						Proc = FurtherProc,
						Data = CurrentEntry.Data,
						EffectiveData = CurrentEntry.EffectiveData
					});
			}

			CallStack.Push (new CallStackEntry
				{
					Proc = NextProc,
					Data = Data,
					EffectiveData = Data ?? CurrentEntry.EffectiveData
				});
		}

		//
		public void ProceedTo<T, F> (Action<WgContext, T> NextProc, T Data, Action<WgContext, F> FurtherProc, uint StartAt = 0)
		{
			ProceedToGeneric (NextProc, Data, FurtherProc, StartAt);
		}

		public void ProceedTo<T> (Action<WgContext, T> NextProc, T Data = default(T))
		{
			ProceedToGeneric (NextProc, Data, null, 0);
		}
	}
}
