using System;

namespace _01_Items
{
	public class S01
	{
		public int X;
		public string S;
		public int N;
	}

	public static class Pieces
	{
		public static void p01 (WgContext Context, S01 Data)
		{
			Data.S = "First Test";

			Context.ProceedTo<S01> (p02);
		}

		public static void p02 (WgContext Context, S01 Data)
		{
			Console.WriteLine ($"Task \"{Data.S}\" is over.");
		}

		public static void For_01_Init (WgContext Context, S01 Data)
		{
			Data.N = 0;
		}

		public static bool For_01_Check (WgContext Context, S01 Data)
		{
			return Data.N < 10;
		}

		public static void For_01_Step (WgContext Context, S01 Data)
		{
			++Data.N;
		}

		public static void For_01_Body (WgContext Context, S01 Data)
		{
			Console.WriteLine ($"f({Data.X}) = {Data.X + Data.N}");
		}
	}
}
