using System.Threading;

namespace _01_Items
{
	class Program
	{
		static void Main (string[] args)
		{
			S01 Data = new S01 { X = 6 };

			WgContext Context = new WgContext ();
			Context.ProceedTo (Pieces.p01, Data);
			Context.Run (new ManualResetEvent (false));
		}
	}
}
