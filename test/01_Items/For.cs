using System;

namespace _01_Items
{
	public class For<T, F> : Loop<T>, IGrain
		where F : For<T, F>, new ()
		where T : CodeBlockDataC
	{
		public Action<WgContext, F> Init;
		public Func<WgContext, F, bool> Check;
		public Action<WgContext, F> Step;
		public Action<WgContext, F> Body;
		public Action<WgContext, T> NextProc;

		public static IGrain Generate (
			Action<WgContext, F> Init,
			Func<WgContext, F, bool> Check,
			Action<WgContext, F> Step,
			Action<WgContext, F> Body,
			Action<WgContext, T> NextProc = null
		)
		{
			F This = new F ();
			This.Init = Init;
			This.Check = Check;
			This.Step = Step;
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

			if (Init != null)
			{
				Context.ProceedTo (Init);
			}
		}

		public static void MakeStep (WgContext Context, F Data)
		{
			bool Proceed = Data.Check?.Invoke (Context, Data) ?? true;

			if (Proceed)
			{
				Context.ProceedTo (MakeStep, Data);

				if (Data.Step != null)
				{
					Context.ProceedTo (Data.Step);
				}

				if (Data.Body != null)
				{
					Context.ProceedTo (Data.Body);
				}
			}
		}
	}
}
