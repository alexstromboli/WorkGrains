using System;

namespace _01_Items
{
	public class While<T, F> : Loop<T>, IGrain
		where F : While<T, F>, new ()
		where T : CodeBlockDataC
	{
		public Func<WgContext, F, bool> Check;
		public Action<WgContext, F> Body;
		public Action<WgContext, T> NextProc;

		public static IGrain Generate (
			Func<WgContext, F, bool> Check,
			Action<WgContext, F> Body,
			Action<WgContext, T> NextProc = null
		)
		{
			F This = new F ();
			This.Check = Check;
			This.Body = Body;
			This.NextProc = NextProc;

			return This;
		}

		public void Append (WgContext Context)
		{
			if (NextProc != null)
			{
				Context.ProceedTo (NextProc);
			}

			Context.ProceedTo (MakeStep, (F)this);
		}

		public static void MakeStep (WgContext Context, F Data)
		{
			bool Proceed = Data.Check?.Invoke (Context, Data) ?? true;

			if (Proceed)
			{
				Context.ProceedTo (MakeStep, Data);

				if (Data.Body != null)
				{
					Context.ProceedTo (Data.Body);
				}
			}
		}
	}
}
