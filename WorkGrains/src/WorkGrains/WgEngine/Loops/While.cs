using System;

// 'While' generic

namespace WorkGrains.Loops
{
	public abstract class While<T, F> : Loop<T>, IGrain     // F is the actual loop type
		where F : While<T, F>, new ()
		where T : CodeBlockDataC
	{
		public Func<WgContext, F, bool> Check;	// loop condition
		public Action<WgContext, F> Body;		// iteration code block

		public static IGrain Generate (
			Func<WgContext, F, bool> Check,
			Action<WgContext, F> Body
		)
		{
			F This = new F ();
			This.Check = Check;
			This.Body = Body;

			return This;
		}

		// first or subsequent iteration scheduling
		protected static void ScheduleNextStep (WgContext Context, F Data)
		{
			Context.ProceedTo (MakeStep, Data, 0, Work.DefaultLoopLabel);
		}

		public void Append (WgContext Context)
		{
			ScheduleNextStep (Context, (F)this);
		}

		public static void MakeStep (WgContext Context, F Data)
		{
			if (Context.IsLoopBreak)
			{
				return;
			}

			// check if condition holds
			bool Proceed = Data.Check?.Invoke (Context, Data) ?? true;

			if (Proceed)
			{
				ScheduleNextStep (Context, Data);

				// iteration body
				if (Data.Body != null)
				{
					Context.ProceedTo (Data.Body);
				}
			}
		}
	}
}
