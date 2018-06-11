using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;

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

	public class CodeBlockDataC
	{
		[JsonIgnore]
		public CodeBlockDataC OuterC;
	}

	public class CodeBlockData<T> : CodeBlockDataC
		where T : CodeBlockDataC
	{
		[JsonIgnore]
		public T Outer
		{
			get { return (T)OuterC; }

			set { OuterC = value; }
		}
	}

	public class CallStackEntry
	{
		[JsonConverter (typeof (DelegateConverter))]
		public Delegate Proc;
		[JsonConverter (typeof (CodeBlockDataConverter))]
		public CodeBlockDataC Data;

		public static CallStackEntry MakeEmpty (CodeBlockDataC Data)
		{
			return new CallStackEntry { Data = Data, Proc = null };
		}

		public override string ToString ()
		{
			return $"{Proc?.Method.DeclaringType.Name ?? "null"}.{Proc?.Method.Name ?? "null"} - {Data?.GetType().Name ?? "null"}";
		}
	}

	[JsonConverter (typeof (WgContextConverter))]
	public partial class WgContext
	{
		public Stack<CallStackEntry> CallStack = new Stack<CallStackEntry> ();

		public void Run (WaitHandle ehStop)
		{
			while (!ehStop.WaitOne (0) && CallStack.Count > 0)
			{
				CallStackEntry CurrentEntry = CallStack.Pop ();

				if (CurrentEntry.Proc == null)
				{
					continue;
				}

				// to keep data in stack
				if (CallStack.Count == 0
				    || !object.ReferenceEquals (CurrentEntry.Data, CallStack.Peek ().Data)
				    )
				{
					CallStack.Push (CallStackEntry.MakeEmpty (CurrentEntry.Data));
				}

				CurrentEntry.Proc.Method.Invoke (null, BindingFlags.Default, null, new[] { this, (object)CurrentEntry.Data }, Thread.CurrentThread.CurrentCulture);

				// DEBUG
				if ((DateTime.Now.Second % 10) > 6)
				{
					break;
				}
			}
		}

		//
		public void ProceedToGeneric (Delegate NextProc, CodeBlockDataC Data, Delegate FurtherProc, uint StartAt = 0)
		{
			CodeBlockDataC LastStackData = CallStack.Count == 0
				? null
				: CallStack.Peek ().Data
				;

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
			where T : CodeBlockDataC
		{
			ProceedToGeneric (NextProc, Data, FurtherProc, StartAt);
		}

		public void ProceedTo<T> (Action<WgContext, T> NextProc, T Data = default(T))
			where T : CodeBlockDataC
		{
			ProceedToGeneric (NextProc, Data, null, 0);
		}

		public void ProceedTo (IGrain Grain)
		{
			Grain.Append (this);
		}
	}
}
