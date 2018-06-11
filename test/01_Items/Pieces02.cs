﻿namespace _01_Items
{
	public class S02 : CodeBlockDataC
	{
		public int Top;
		public int Sum;

		public class _while01 : While<S02, _while01>
		{
		}
	}

	public static class Pieces02
	{
		public static void p01 (WgContext Context, S02 Data)
		{
			Data.Sum = 0;
			Context.ProceedTo (S02._while01.Generate (While_01_Check, While_01_Body, p02));
		}

		public static void p02 (WgContext Context, S02 Data)
		{
			//System.IO.File.WriteAllText ("out.txt", string.Join (" ", Data.Numbers));
		}

		//
		public static bool While_01_Check (WgContext Context, S02._while01 Data)
		{
			return Data.Outer.Top > 0;
		}

		public static void While_01_Body (WgContext Context, S02._while01 Data)
		{
			Data.Outer.Sum += Data.Outer.Top;
			--Data.Outer.Top;
		}
	}
}
