using System;
using System.Reflection;

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
		protected static GrainInfo NameForDelegate (Delegate Block, Type T)
		{
			return new GrainInfo
				{
					MethodAssembly = Block.Method.DeclaringType.Assembly.FullName,
					MethodClass = Block.Method.DeclaringType.FullName,
					MethodName = Block.Method.Name,
					DataAssembly = T.Assembly.FullName,
					DataType = T.FullName
				};
		}

		public static GrainInfo NameForAction<T> (Action<WgContext, T> Block)
		{
			return NameForDelegate (Block, typeof(T));
		}

		public static GrainInfo NameForPredicate<T> (Func<WgContext, T, bool> Block)
		{
			return NameForDelegate (Block, typeof (T));
		}

		public static Delegate DelegateForName (GrainInfo Info, Func<Type, Type> DelTypeMaker)
		{
			Assembly MethodAssembly = Assembly.Load (Info.MethodAssembly);
			Type MethodClass = MethodAssembly.GetType (Info.MethodClass);

			Assembly DataAssembly = Assembly.Load (Info.DataAssembly);
			Type DataType = DataAssembly.GetType (Info.DataType);

			Type ActionType = DelTypeMaker (DataType);
			Delegate Proc = Delegate.CreateDelegate (ActionType, MethodClass, Info.MethodName, false, false);

			return Proc;
		}

		public static Action<WgContext, T> ActionForName<T> (GrainInfo Info)
		{
			Action<WgContext, T> Proc = (Action<WgContext, T>)DelegateForName (Info, DT => typeof (Action<,>).MakeGenericType (typeof (WgContext), DT));

			return Proc;
		}

		public static Func<WgContext, T, bool> PredicateForName<T> (GrainInfo Info)
		{
			Func<WgContext, T, bool> Proc = (Func<WgContext, T, bool>)DelegateForName (Info, DT => typeof (Func<,,>).MakeGenericType (typeof (WgContext), DT, typeof (bool)));

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
	}
}
