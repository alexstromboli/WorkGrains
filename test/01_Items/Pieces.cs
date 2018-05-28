using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace _01_Items
{
	public class GrainInfo
	{
		public string MethodAssembly;
		public string MethodClass;
		public string MethodName;

		public string DataAssembly;
		public string DataType;
	}

	public class WgContext
	{
		public static GrainInfo NameForAction<T> (Action<WgContext, T> Block)
		{
			return new GrainInfo
				{
					MethodAssembly = Block.Method.DeclaringType.Assembly.FullName,
					MethodClass = Block.Method.DeclaringType.FullName,
					MethodName = Block.Method.Name,
					DataAssembly = typeof(T).Assembly.FullName,
					DataType = typeof(T).FullName
				};
		}

		public static Action<WgContext, T> ActionForName<T> (GrainInfo Info)
		{
			Assembly MethodAssembly = Assembly.Load (Info.MethodAssembly);
			Type MethodClass = MethodAssembly.GetType (Info.MethodClass);

			Assembly DataAssembly = Assembly.Load (Info.DataAssembly);
			Type DataType = DataAssembly.GetType (Info.DataType);

			Type ActionType = typeof(Action<,>).MakeGenericType (typeof(WgContext), DataType);
			Action<WgContext, T> Proc = (Action<WgContext, T>)Delegate.CreateDelegate (ActionType, MethodClass, Info.MethodName, false, false);

			return Proc;
		}
	}

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
			++Data.X;
		}

		public static void For_01_Init (S01 Data)
		{
			Data.N = 0;
		}

		public static bool For_01_Check (S01 Data)
		{
			return Data.N < 0;
		}

		public static void For_01_Step (S01 Data)
		{
			++Data.N;
		}
	}
}
