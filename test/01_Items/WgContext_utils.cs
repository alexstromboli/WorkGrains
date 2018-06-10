using System;
using System.Reflection;

namespace _01_Items
{
	public partial class WgContext
	{
		public static Type GetIfAction (Type DelegateType)
		{
			return DelegateType.IsGenericType
					&& DelegateType.GetGenericTypeDefinition () == typeof (Action<,>)
					&& DelegateType.GenericTypeArguments[0] == typeof (WgContext)
				? DelegateType.GenericTypeArguments[1]
				: null
				;
		}

		public static Type GetIfPredicate (Type DelegateType)
		{
			return DelegateType.IsGenericType
					&& DelegateType.GetGenericTypeDefinition () == typeof (Func<,,>)
					&& DelegateType.GenericTypeArguments[0] == typeof (WgContext)
					&& DelegateType.GenericTypeArguments[2] == typeof (bool)
				? DelegateType.GenericTypeArguments[1]
				: null
				;
		}

		//
		public static GrainInfo NameForDelegate (Delegate Block, Type T)
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

		protected static Delegate DelegateForName (GrainInfo Info, Func<Type, Type> DelTypeMaker)
		{
			if (Info == null)
			{
				return null;
			}

			Assembly MethodAssembly = Assembly.Load (Info.MethodAssembly);
			Type MethodClass = MethodAssembly.GetType (Info.MethodClass);

			Assembly DataAssembly = Assembly.Load (Info.DataAssembly);
			Type DataType = DataAssembly.GetType (Info.DataType);

			Type ActionType = DelTypeMaker (DataType);
			Delegate Proc = Delegate.CreateDelegate (ActionType, MethodClass, Info.MethodName, false, false);

			return Proc;
		}

		public static Delegate ActionForName (GrainInfo Info)
		{
			Delegate Proc = DelegateForName (Info, DT => typeof (Action<,>).MakeGenericType (typeof (WgContext), DT));

			return Proc;
		}

		public static Delegate PredicateForName (GrainInfo Info)
		{
			Delegate Proc = DelegateForName (Info, DT => typeof (Func<,,>).MakeGenericType (typeof (WgContext), DT, typeof (bool)));

			return Proc;
		}
	}
}
