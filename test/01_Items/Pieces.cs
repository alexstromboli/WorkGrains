using System;

namespace _01_Items
{
	public class S01
	{
		public int[] Numbers;

		public class _for01 : For<S01, _for01>
		{
			public int i;
		}

		public class _for02 : For<_for01, _for02>
		{
			public int j;
		}
	}

	public static class Pieces
	{
		public static void p01 (WgContext Context, S01 Data)
		{
			Context.ProceedTo (S01._for01.Generate (For_01_Init, For_01_Check, For_01_Step, For_01_Body, p02));
		}

		public static void p02 (WgContext Context, S01 Data)
		{
			//Console.WriteLine ($"Task \"{Data.S}\" is over.");
		}

		//
		public static void For_01_Init (WgContext Context, S01._for01 Data)
		{
			Data.i = 0;
		}

		public static bool For_01_Check (WgContext Context, S01._for01 Data)
		{
			S01 Outer = Data.GetOuterData (Context);
			return Data.i < Outer.Numbers.Length - 1;
		}

		public static void For_01_Step (WgContext Context, S01._for01 Data)
		{
			++Data.i;
		}

		public static void For_01_Body (WgContext Context, S01._for01 Data)
		{
			Context.ProceedTo (S01._for02.Generate (For_02_Init, For_02_Check, For_02_Step, For_02_Body));
		}

		//
		public static void For_02_Init (WgContext Context, S01._for02 Data)
		{
			S01._for01 OuterFor = Data.GetOuterData (Context);
			Data.j = OuterFor.i + 1;
		}

		public static bool For_02_Check (WgContext Context, S01._for02 Data)
		{
			S01 MainBodyData = Data.GetOuterData (Context).GetOuterData (Context);
			return Data.j < MainBodyData.Numbers.Length;
		}

		public static void For_02_Step (WgContext Context, S01._for02 Data)
		{
			++Data.j;
		}

		public static void For_02_Body (WgContext Context, S01._for02 Data)
		{
			S01._for01 OuterFor = Data.GetOuterData (Context);
			S01 MainBodyData = Data.GetOuterData (Context).GetOuterData (Context);

			if (MainBodyData.Numbers[Data.j] < MainBodyData.Numbers[OuterFor.i])
			{
				int t = MainBodyData.Numbers[Data.j];
				MainBodyData.Numbers[Data.j] = MainBodyData.Numbers[OuterFor.i];
				MainBodyData.Numbers[OuterFor.i] = t;
			}
		}
	}
}
