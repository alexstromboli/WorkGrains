using System;
using System.Linq;
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
		public object Data;
	}

	public interface IGrain
	{
		void Append (WgContext Context);
	}

	public class For<T, F> : IGrain where F : For<T, F>, new () where T : class
	{
		public int OuterBlockDataDepth { get; protected set; }
		protected Action<WgContext, F> Init;
		protected Func<WgContext, F, bool> Check;
		protected Action<WgContext, F> Step;
		protected Action<WgContext, F> Body;
		protected Action<WgContext, T> NextProc;

		public static IGrain Generate (
			Action<WgContext, F> Init,
			Func<WgContext, F, bool> Check,
			Action<WgContext, F> Step,
			Action<WgContext, F> Body,
			Action<WgContext, T> NextProc
			)
		{
			F This = new F ();
			This.Init = Init;
			This.Check = Check;
			This.Step = Step;
			This.Body = Body;
			This.NextProc = NextProc;

			return This;
		}

		public void Append (WgContext Context)
		{
			OuterBlockDataDepth = Context.CallStackDepth;

			if (NextProc != null)
			{
				Context.ProceedTo (NextProc);
			}

			Context.ProceedTo (MakeStep, (F)this);

			if (Init != null)
			{
				Context.ProceedTo (Init);
			}
		}

		public static void MakeStep (WgContext Context, F Data)
		{
			bool Proceed = Data.Check (Context, Data);

			if (Proceed)
			{
				Context.ProceedTo (MakeStep, Data);

				if (Data.Step != null)
				{
					Context.ProceedTo (Data.Step);
				}

				if (Data.Body != null)
				{
					Context.ProceedTo (Data.Body);
				}
			}
		}
	}

	public partial class WgContext
	{
		//
		protected Stack<CallStackEntry> CallStack = new Stack<CallStackEntry> ();
		protected CallStackEntry CurrentEntry;
		public bool IsLast { get; protected set; } = true;
		public int CallStackDepth => CallStack.Count;
		public CallStackEntry AtDepth (int Depth) => CallStack.Skip (CallStack.Count - Depth - 1).First ();

		protected CallStackEntry GetPrevEntry (int Depth = 1)
		{
			if (Depth < 0 || Depth > CallStack.Count)
			{
				return null;
			}

			CallStackEntry Entry = Depth == 0
				? CurrentEntry
				: CallStack.Skip (Depth - 1).First ()
				;

			return Entry;
		}

		public void Run (WaitHandle ehStop)
		{
			while (!ehStop.WaitOne (0) && CallStack.Count > 0)
			{
				CurrentEntry = CallStack.Pop ();
				IsLast = true;
				CurrentEntry.Proc.Method.Invoke (null, BindingFlags.Default, null, new[] { this, CurrentEntry.Data }, Thread.CurrentThread.CurrentCulture);
			}
		}

		//
		public void ProceedToGeneric (Delegate NextProc, object Data, Delegate FurtherProc, uint StartAt = 0)
		{
			object LastStackData = GetPrevEntry ()?.Data;

			if (Data == null)
			{
				if (IsLast)
				{
					Data = CurrentEntry.Data;
				}
				else
				{
					Data = LastStackData;
				}
			}

			if (FurtherProc != null)
			{
				CallStack.Push (new CallStackEntry
					{
						Proc = FurtherProc,
						Data = LastStackData
					});
			}

			CallStack.Push (new CallStackEntry
				{
					Proc = NextProc,
					Data = Data
				});

			IsLast = false;
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

		public void ProceedTo (IGrain Grain)
		{
			Grain.Append (this);
		}
	}
}
