using System;

namespace _01_Items
{
	public class While<T, F> : Loop<T>, IGrain
		where F : While<T, F>, new ()
		where T : CodeBlockDataC
	{
		public Func<WgContext, F, bool> Check;
		public Action<WgContext, F> Body;

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

		protected static void ScheduleNextStep (WgContext Context, F Data)
		{
			Context.ProceedTo (MakeStep, Data, 0, WgContext.DefaultLoopLabel);
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

			bool Proceed = Data.Check?.Invoke (Context, Data) ?? true;

			if (Proceed)
			{
				ScheduleNextStep (Context, Data);

				if (Data.Body != null)
				{
					Context.ProceedTo (Data.Body);
				}
			}
		}
	}
}
