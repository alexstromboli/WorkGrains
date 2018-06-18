using System;
using System.Reflection;

// delegates analysis
// serialization/deserialization

namespace WorkGrains
{
	public partial class WgContext
	{
		// if DelegateType is Action<WgContext,T>, then find and return T
		// else null
		public static Type GetIfAction (Type DelegateType)
		{
			return DelegateType.IsGenericType
					&& DelegateType.GetGenericTypeDefinition () == typeof (Action<,>)
					&& DelegateType.GenericTypeArguments[0] == typeof (WgContext)
				? DelegateType.GenericTypeArguments[1]
				: null
				;
		}

		// if DelegateType is Func<WgContext,T,bool>, then find and return T
		// else null
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

		// delegate serialization
		public static GrainInfo InfoFromDelegate (Delegate Proc, Type T)
		{
			return new GrainInfo
				{
					MethodAssembly = Proc.Method.DeclaringType.Assembly.FullName,
					MethodClass = Proc.Method.DeclaringType.FullName,
					MethodName = Proc.Method.Name,
					DataAssembly = T.Assembly.FullName,
					DataType = T.FullName
				};
		}

		// delegate deserialization
		protected static Delegate DelegateFromInfo (GrainInfo Info, Func<Type, Type> DelTypeMaker)
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
			Delegate Proc = DelegateFromInfo (Info, DT => typeof (Action<,>).MakeGenericType (typeof (WgContext), DT));

			return Proc;
		}

		public static Delegate PredicateForName (GrainInfo Info)
		{
			Delegate Proc = DelegateFromInfo (Info, DT => typeof (Func<,,>).MakeGenericType (typeof (WgContext), DT, typeof (bool)));

			return Proc;
		}
	}
}
