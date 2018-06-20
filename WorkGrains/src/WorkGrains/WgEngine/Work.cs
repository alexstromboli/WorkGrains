using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;

using WorkGrains.Converters;

namespace WorkGrains
{
	public class Work
	{
		public static readonly string DefaultLoopLabel = "";

		public Guid Id;

		[JsonIgnore]
		public WorkSchedule Schedule;

		[JsonIgnore]
		public WgEngine Engine;

		// call stack
		[JsonConverter (typeof (CallStackEntryStackConverter))]
		public Stack<CallStackEntry> CallStack = new Stack<CallStackEntry> ();

		// 'break' or 'continue'
		[JsonProperty (NullValueHandling = NullValueHandling.Ignore)]
		public LeapInfo Leap;

		// execution loop
		public void Run (WaitHandle ehStop)
		{
			WgContext Context = new WgContext (this, Engine);

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
					CurrentEntry.Proc.Method.Invoke (null, BindingFlags.Default, null, new[] { Context, (object)CurrentEntry.Data },
						Thread.CurrentThread.CurrentCulture);

					Leap = null;
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException is WgLoopException)
					{
						Leap = ((WgLoopException)ex.InnerException).LeapInfo;
					}
					else
					{
						// here: handle, store and forward
						throw;
					}
				}

				// DEBUG
				if ((DateTime.Now.Second % 10) > 6)
				{
					break;
				}
			}
		}

		// push next action
		public void ProceedToGeneric (Delegate NextProc, CodeBlockDataC Data, WorkSchedule StartAt = null, string LoopHeader = null)
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

			CallStack.Push (new CallStackEntry
				{
					Proc = NextProc,
					Data = Data,
					LoopHeader = LoopHeader
				});
		}

		// push next action, typified
		public void ProceedTo<T> (Action<WgContext, T> NextProc, T Data = null, WorkSchedule StartAt = null, string LoopHeader = null)
			where T : CodeBlockDataC
		{
			ProceedToGeneric (NextProc, Data, StartAt, LoopHeader);
		}

		// postpone
		public void RescheduleWork ()
		{
			throw new NotImplementedException ();
		}
	}
}
