namespace _01_Items
{
	public class S01 : InnerBlockC
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
			System.IO.File.WriteAllText ("out.txt", string.Join (" ", Data.Numbers));
		}

		//
		public static void For_01_Init (WgContext Context, S01._for01 Data)
		{
			Data.i = 0;
		}

		public static bool For_01_Check (WgContext Context, S01._for01 Data)
		{
			return Data.i < Data.Outer.Numbers.Length - 1;
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
			Data.j = Data.Outer.i + 1;
		}

		public static bool For_02_Check (WgContext Context, S01._for02 Data)
		{
			return Data.j < Data.Outer.Outer.Numbers.Length;
		}

		public static void For_02_Step (WgContext Context, S01._for02 Data)
		{
			++Data.j;
		}

		public static void For_02_Body (WgContext Context, S01._for02 Data)
		{
			if (Data.Outer.Outer.Numbers[Data.j] < Data.Outer.Outer.Numbers[Data.Outer.i])
			{
				int t = Data.Outer.Outer.Numbers[Data.j];
				Data.Outer.Outer.Numbers[Data.j] = Data.Outer.Outer.Numbers[Data.Outer.i];
				Data.Outer.Outer.Numbers[Data.Outer.i] = t;
			}

			// DEBUG
			System.Threading.Thread.Sleep (500);
		}
	}
}
