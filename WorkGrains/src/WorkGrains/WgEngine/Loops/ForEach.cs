using System;
using System.Collections.Generic;

using Newtonsoft.Json;

// 'ForEach' generic
// enumerates an array, not a generic IEnumerable, since the container needs to be serializable

namespace WorkGrains.Loops
{
	// F is the actual loop type
	// CNT is array type
	// EL is array element type
	// T is outer block data type
	public abstract class ForEach<T, CNT, EL, F> : Loop<T>, IGrain
		where T : CodeBlockDataC
		where CNT : class, IList<EL>
		where F : ForEach<T, CNT, EL, F>, new ()
	{
		public Action<WgContext, F> GetContainer;
		public Action<WgContext, F> Step;
		public Action<WgContext, F> Body;
		public int CurrentIndex;
		public EL CurrentElement;

		[JsonIgnore]
		public CNT Container;

		public static IGrain Generate (
			Action<WgContext, F> GetContainer,
			Action<WgContext, F> Step,
			Action<WgContext, F> Body
		)
		{
			F This = new F ();
			This.GetContainer = GetContainer;
			This.Step = Step;
			This.Body = Body;
			This.Container = null;

			return This;
		}

		// first or subsequent iteration scheduling
		protected static void ScheduleNextStep (WgContext Context, F Data)
		{
			Context.ProceedTo (MakeStep, Data, null, Work.DefaultLoopLabel);
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

			// retrieve container
			if (Data.Container == null)
			{
				Data.GetContainer (Context, Data);
			}

			if (Data.Container == null)
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
