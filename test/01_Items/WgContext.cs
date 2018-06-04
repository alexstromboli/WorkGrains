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

	public interface IGrain
	{
		void Append (WgContext Context);
	}

	public class InnerBlockC
	{
		public object OuterC;
	}

	public class InnerBlock<T> : InnerBlockC
	{
		public T Outer
		{
			get { return (T)OuterC; }

			set { OuterC = value; }
		}
	}

	public class CallStackEntry
	{
		public Delegate Proc;
		public InnerBlockC Data;

		public static CallStackEntry MakeEmpty (InnerBlockC Data)
		{
			return new CallStackEntry { Data = Data, Proc = null };
		}

		public override string ToString ()
		{
			return $"{Proc?.Method.DeclaringType.Name ?? "null"}.{Proc?.Method.Name ?? "null"} - {Data.GetType().Name}";
		}
	}

	public class For<T, F> : InnerBlock<T>, IGrain
		where F : For<T, F>, new ()
		where T : InnerBlockC
	{
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
			Action<WgContext, T> NextProc = null
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

				if (CurrentEntry.Proc == null)
				{
					continue;
				}

				// to keep data in stack
				CallStack.Push (CallStackEntry.MakeEmpty (CurrentEntry.Data));

				CurrentEntry.Proc.Method.Invoke (null, BindingFlags.Default, null, new[] { this, (object)CurrentEntry.Data }, Thread.CurrentThread.CurrentCulture);
			}
		}

		//
		public void ProceedToGeneric (Delegate NextProc, InnerBlockC Data, Delegate FurtherProc, uint StartAt = 0)
		{
			InnerBlockC LastStackData = GetPrevEntry ()?.Data;

			if (Data == null)
			{
				Data = LastStackData;
			}

			Data.OuterC = object.ReferenceEquals (Data, LastStackData)
				? LastStackData.OuterC
				: LastStackData
				;

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
		}

		//
		public void ProceedTo<T, F> (Action<WgContext, T> NextProc, T Data, Action<WgContext, F> FurtherProc, uint StartAt = 0)
			where T : InnerBlockC
		{
			ProceedToGeneric (NextProc, Data, FurtherProc, StartAt);
		}

		public void ProceedTo<T> (Action<WgContext, T> NextProc, T Data = default(T))
			where T : InnerBlockC
		{
			ProceedToGeneric (NextProc, Data, null, 0);
		}

		public void ProceedTo (IGrain Grain)
		{
			Grain.Append (this);
		}
	}
}
