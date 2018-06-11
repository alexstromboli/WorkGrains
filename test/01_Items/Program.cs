using System;
using System.IO;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

namespace _01_Items
{
	class Program
	{
		static void Main (string[] args)
		{
			WgContext Context;
			DelegateConverter DelConv = new DelegateConverter ();

			//
			string StateFilePath = Path.GetFullPath ("state.json");
			Encoding enc = Encoding.UTF8;
			if (File.Exists (StateFilePath))
			{
				string StateJson = File.ReadAllText (StateFilePath, enc);
				Context = JsonConvert.DeserializeObject<WgContext> (StateJson, DelConv);
			}
			else
			{
				Context = new WgContext ();
				//S01 Data = new S01 { Numbers = new[] { 5, 3, 8, 10, 1, 7, 4, 2 } };
				//Context.ProceedTo (Pieces01.p01, Data);

				S02 Data = new S02 { Top = DateTime.Now.Second };
				Context.ProceedTo (Pieces02.p01, Data);
			}

			Context.Run (new ManualResetEvent (false));

			{
				string StateJson = JsonConvert.SerializeObject (Context, DelConv);
				File.WriteAllText (StateFilePath, StateJson, enc);
			}
		}
	}
}
