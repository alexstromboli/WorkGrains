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
		[JsonProperty (NullValueHandling = NullValueHandling.Ignore)]
		public string LoopHeader;

		public static CallStackEntry MakeEmpty (CodeBlockDataC Data)
		{
			return new CallStackEntry { Data = Data, Proc = null };
		}

		public override string ToString ()
		{
			return $"{Proc?.Method.DeclaringType.Name ?? "null"}.{Proc?.Method.Name ?? "null"} - {Data?.GetType().Name ?? "null"}";
		}
	}

	public class WgLoopException : Exception
	{
		public enum LeapType
		{
			Break,
			Continue
		}

		public LeapType Type;
		public string LoopHeader;

		public WgLoopException (LeapType Type, string LoopHeader)
		{
			this.Type = Type;
			this.LoopHeader = LoopHeader ?? WgContext.DefaultLoopLabel;
		}

		public WgLoopException ()
			: this (LeapType.Break, WgContext.DefaultLoopLabel)
		{
		}
	}

	[JsonConverter (typeof (WgContextConverter))]
	public partial class WgContext
	{
		public static readonly string DefaultLoopLabel = "";

		public Stack<CallStackEntry> CallStack = new Stack<CallStackEntry> ();
		public WgLoopException Leap = null;

		public bool IsLoopBreak => Leap != null && Leap.Type == WgLoopException.LeapType.Break;
		public bool IsLoopContinue => Leap != null && Leap.Type == WgLoopException.LeapType.Continue;

		public void Run (WaitHandle ehStop)
		{
			while (!ehStop.WaitOne (0) && CallStack.Count > 0)
			{
				CallStackEntry CurrentEntry = CallStack.Pop ();

				bool MustSkipForLeap = Leap != null
					&& !(CurrentEntry.LoopHeader != null && Leap.LoopHeader == DefaultLoopLabel)
					&& !(CurrentEntry.LoopHeader != null && Leap.LoopHeader == CurrentEntry.LoopHeader)
					;

				if (CurrentEntry.Proc == null || MustSkipForLeap)
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

				try
				{
					CurrentEntry.Proc.Method.Invoke (null, BindingFlags.Default, null, new[] {this, (object)CurrentEntry.Data},
						Thread.CurrentThread.CurrentCulture);

					Leap = null;
				}
				catch (WgLoopException ex)
				{
					Leap = ex;
				}

				// DEBUG
				if ((DateTime.Now.Second % 10) > 6)
				{
					break;
				}
			}
		}

		public void LoopBreak (string LoopHeader = null)
		{
			throw new WgLoopException (WgLoopException.LeapType.Break, LoopHeader ?? DefaultLoopLabel);
		}

		public void LoopContinue (string LoopHeader = null)
		{
			throw new WgLoopException (WgLoopException.LeapType.Continue, LoopHeader ?? DefaultLoopLabel);
		}

		//
		public void ProceedToGeneric (Delegate NextProc, CodeBlockDataC Data, Delegate FurtherProc, uint StartAt = 0, string LoopHeader = null)
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
					Data = Data,
					LoopHeader = LoopHeader
				});
		}

		//
		public void ProceedTo<T, F> (Action<WgContext, T> NextProc, T Data, Action<WgContext, F> FurtherProc, uint StartAt = 0, string LoopHeader = null)
			where T : CodeBlockDataC
		{
			ProceedToGeneric (NextProc, Data, FurtherProc, StartAt, LoopHeader);
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
