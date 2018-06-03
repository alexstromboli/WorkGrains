using System.Threading;

namespace _01_Items
{
	class Program
	{
		static void Main (string[] args)
		{
			S01 Data = new S01 {Numbers = new[] {5, 3, 8, 10, 1}};

			WgContext Context = new WgContext ();
			Context.ProceedTo (Pieces.p01, Data);
			Context.Run (new ManualResetEvent (false));
		}
	}
}
