using System;

namespace WorkGrains
{
	// code block execution context
	public partial class WgContext
	{
		// 'break' or 'continue' flags
		public bool IsLoopBreak => Work.Leap != null && Work.Leap.Type == LeapInfo.LeapType.Break;
		public bool IsLoopContinue => Work.Leap != null && Work.Leap.Type == LeapInfo.LeapType.Continue;

		protected Work Work;
		protected WgEngine Engine;

		public WgContext (Work Work, WgEngine Engine)
		{
			this.Work = Work;
			this.Engine = Engine;
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
		public void ProceedToGeneric (Delegate NextProc, CodeBlockDataC Data, bool IsFinalizing = false, string LoopHeader = null)
		{
			Work.ProceedToGeneric (NextProc, Data, IsFinalizing, LoopHeader);
		}

		// push next action, typified
		public void ProceedTo<T> (Action<WgContext, T> NextProc, T Data = null, bool IsFinalizing = false, string LoopHeader = null)
			where T : CodeBlockDataC
		{
			Work.ProceedTo (NextProc, Data, IsFinalizing, LoopHeader);
		}

		// push next action-aware block
		public void ProceedTo (IGrain Grain)
		{
			Grain.Append (this);
		}

		// postpone
		public void RescheduleWork ()
		{
			throw new NotImplementedException ();
		}
	}
}
