using System;

// 'For' generic

namespace _01_Items
{
	public abstract class For<T, F> : Loop<T>, IGrain		// F is the actual loop type
		where F : For<T, F>, new ()
		where T : CodeBlockDataC
	{
		public Action<WgContext, F> Init;
		public Func<WgContext, F, bool> Check;
		public Action<WgContext, F> Step;
		public Action<WgContext, F> Body;

		public static IGrain Generate (
			Action<WgContext, F> Init,
			Func<WgContext, F, bool> Check,
			Action<WgContext, F> Step,
			Action<WgContext, F> Body
		)
		{
			F This = new F ();
			This.Init = Init;
			This.Check = Check;
			This.Step = Step;
			This.Body = Body;

			return This;
		}

		// first or subsequent iteration scheduling
		protected static void ScheduleNextStep (WgContext Context, F Data)
		{
			Context.ProceedTo (MakeStep, Data, 0, WgContext.DefaultLoopLabel);
		}

		public void Append (WgContext Context)
		{
			ScheduleNextStep (Context, (F)this);

			// initialization
			if (Init != null)
			{
				Context.ProceedTo (Init);
			}
		}

		// loop header
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

				// increment
				if (Data.Step != null)
				{
					Context.ProceedTo (Data.Step);
				}

				// iteration body
				if (Data.Body != null)
				{
					Context.ProceedTo (Data.Body);
				}
			}
		}
	}
}
