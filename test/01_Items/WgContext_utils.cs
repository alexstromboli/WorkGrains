using System;
using System.Reflection;

namespace _01_Items
{
	public partial class WgContext
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
}
