using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace _01_Items
{
	class Program
	{
		static void Main (string[] args)
		{
			var ProcInfo = WgContext.NameForAction<S01> (Pieces.p01);

			Action<WgContext, S01> action02 = WgContext.ActionForName<S01> (ProcInfo);

			S01 Data = new S01 { X = 7 };
			action02 (null, Data);
		}
	}
}
