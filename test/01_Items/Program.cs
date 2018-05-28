using System;

namespace _01_Items
{
	class Program
	{
		static void Main (string[] args)
		{
			S01 Data = new S01 { X = 7, N = 11 };

			{
				var ProcInfo = WgContext.NameForAction<S01> (Pieces.p01);
				Action<WgContext, S01> action02 = WgContext.ActionForName<S01> (ProcInfo);

				action02 (null, Data);
			}

			{
				var ProcInfo = WgContext.NameForPredicate<S01> (Pieces.For_01_Check);
				Func<WgContext, S01, bool> action02 = WgContext.PredicateForName<S01> (ProcInfo);

				bool B = action02 (null, Data);
			}
		}
	}
}
