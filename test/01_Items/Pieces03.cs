using System.Collections.Generic;

// 'ForEach' test

namespace _01_Items
{
	public class S03 : CodeBlockDataC
	{
		public List<string> Lines;
		public List<string> Result;

		public class _foreach01 : ForEach<S03, List<string>, string, _foreach01>
		{
			public string s;
		}
	}

	public static class Pieces03
	{
		public static void p01 (WgContext Context, S03 Data)
		{
			// data
			Data.Result = new List<string> ();

			// stacking sub-blocks
			Context.ProceedTo<S03> (p02);
			Context.ProceedTo (S03._foreach01.Generate (ForEach_01_GetContainer, ForEach_01_Step, ForEach_01_Body));
		}

		public static void p02 (WgContext Context, S03 Data)
		{
			System.IO.File.WriteAllLines ("out.txt", Data.Result);
		}

		//
		public static void ForEach_01_GetContainer (WgContext Context, S03._foreach01 Data)
		{
			Data.Container = Data.Outer.Lines;
		}

		public static void ForEach_01_Step (WgContext Context, S03._foreach01 Data)
		{
			Data.s = Data.CurrentElement;
		}

		public static void ForEach_01_Body (WgContext Context, S03._foreach01 Data)
		{
			if (Data.s.Length > 4)
			{
				Data.Outer.Result.Add (Data.s);
			}
		}
	}
}
