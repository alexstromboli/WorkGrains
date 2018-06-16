using System;
using System.Collections.Generic;

// 'ForEach' generic
// enumerates an array, not a generic IEnumerable, since the container needs to be serializable

namespace _01_Items
{
	// F is the actual loop type
	// CNT is array type
	// EL is array element type
	// T is outer block data type
	public abstract class ForEach<T, CNT, EL, F> : Loop<T>, IGrain
		where T : CodeBlockDataC
		where CNT : IList<EL>
		where F : ForEach<T, CNT, EL, F>, new ()
	{
		public CNT Container;
		public Action<WgContext, F> Step;
		public Action<WgContext, F> Body;
		public int CurrentIndex;
		public EL CurrentElement;

		public static IGrain Generate (
			CNT Container,
			Action<WgContext, F> Step,
			Action<WgContext, F> Body
		)
		{
			F This = new F ();
			This.Container = Container;
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
			CurrentIndex = -1;
			ScheduleNextStep (Context, (F)this);
		}

		// loop header
		public static void MakeStep (WgContext Context, F Data)
		{
			if (Context.IsLoopBreak)
			{
				return;
			}

			// check if continuation condition holds
			++Data.CurrentIndex;
			bool Proceed = Data.CurrentIndex < Data.Container.Count;

			if (Proceed)
			{
				ScheduleNextStep (Context, Data);

				// increment
				Data.CurrentElement = Data.Container[Data.CurrentIndex];

				// iteration body
				if (Data.Body != null)
				{
					Context.ProceedTo (Data.Body);
				}

				// step
				if (Data.Step != null)
				{
					Context.ProceedTo (Data.Step);
				}
			}
		}
	}
}
