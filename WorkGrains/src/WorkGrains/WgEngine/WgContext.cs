using System;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

namespace WorkGrains
{
	// code block execution context
	public partial class WgContext
	{
		// 'break' or 'continue' flags
		public bool IsLoopBreak => Leap != null && Leap.Type == LeapInfo.LeapType.Break;
		public bool IsLoopContinue => Leap != null && Leap.Type == LeapInfo.LeapType.Continue;

		protected Work Work;
		protected Stack<CallStackEntry> CallStack;
		protected LeapInfo Leap;

		public WgContext (Work Work)
		{
			this.Work = Work;
			this.CallStack = Work.CallStack;
			this.Leap = Work.Leap;
		}

		// execution loop
		public void Run (WaitHandle ehStop)
		{
			while (!ehStop.WaitOne (0) && CallStack.Count > 0)
			{
				CallStackEntry CurrentEntry = CallStack.Pop ();

				bool MustSkipForLeap = Leap != null
					&& !(CurrentEntry.LoopHeader != null && Leap.LoopHeader == Work.DefaultLoopLabel)
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

		public void LoopBreak (string LoopHeader = null)
		{
			throw new WgLoopException (LeapInfo.LeapType.Break, LoopHeader ?? Work.DefaultLoopLabel);
		}

		public void LoopContinue (string LoopHeader = null)
		{
			throw new WgLoopException (LeapInfo.LeapType.Continue, LoopHeader ?? Work.DefaultLoopLabel);
		}

		// push next action
		public void ProceedToGeneric (Delegate NextProc, CodeBlockDataC Data, uint StartAt = 0, string LoopHeader = null)
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
		public void ProceedTo<T> (Action<WgContext, T> NextProc, T Data = null, uint StartAt = 0, string LoopHeader = null)
			where T : CodeBlockDataC
		{
			ProceedToGeneric (NextProc, Data, StartAt, LoopHeader);
		}

		// push next action-aware block
		public void ProceedTo (IGrain Grain)
		{
			Grain.Append (this);
		}
	}
}
