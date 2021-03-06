﻿using System;
using System.Collections.Generic;

// 'While' test

namespace _01_Items
{
	public class S02 : CodeBlockDataC
	{
		public int Top;
		public int Sum;
		public List<int> Numbers;

		public class _while01 : While<S02, _while01>
		{
		}
	}

	public static class Pieces02
	{
		public static void p01 (WgContext Context, S02 Data)
		{
			// data
			Data.Sum = 0;
			Data.Numbers = new List<int> ();

			// stacking sub-blocks
			Context.ProceedTo<S02> (p02);
			Context.ProceedTo (S02._while01.Generate (While_01_Check, While_01_Body));
		}

		public static void p02 (WgContext Context, S02 Data)
		{
			System.IO.File.WriteAllText ("out.txt", string.Join (" ", Data.Numbers));
		}

		//
		public static bool While_01_Check (WgContext Context, S02._while01 Data)
		{
			return Data.Outer.Top > 0;
		}

		public static void While_01_Body (WgContext Context, S02._while01 Data)
		{
			--Data.Outer.Top;

			if ((Data.Outer.Top % 5) == 0)
			{
				Context.LoopContinue ();
			}

			Data.Outer.Sum += Data.Outer.Top;
			Data.Outer.Numbers.Add (Data.Outer.Top);

			Context.ProceedTo<S02._while01> (While_01_Dummy);
			Context.ProceedTo<S02._while01> (While_01_SubProc);
		}

		public static void While_01_SubProc (WgContext Context, S02._while01 Data)
		{
			if ((Data.Outer.Top % 12) == 11)
			{
				Context.LoopBreak ();
			}
		}

		public static void While_01_Dummy (WgContext Context, S02._while01 Data)
		{
			Console.WriteLine ("iteration trail for " + Data.Outer.Top);
		}
	}
}
