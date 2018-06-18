using System;

using Newtonsoft.Json;

namespace WorkGrains.Converters
{
	// stack entry delegate (de-)serializer
	public class DelegateConverter : JsonConverter
	{
		public override bool CanConvert (Type objectType)
		{
			return WgContext.GetIfAction (objectType) != null
			       || WgContext.GetIfPredicate (objectType) != null
				;
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			Type ValueType = value.GetType ();
			Type DataType = WgContext.GetIfAction (ValueType) ?? WgContext.GetIfPredicate (ValueType);
			GrainInfo Info = WgContext.InfoFromDelegate ((Delegate)value, DataType);
			serializer.Serialize (writer, Info);
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			GrainInfo Info = serializer.Deserialize<GrainInfo> (reader);
			Delegate Proc = null;
			Type DataType = null;

			DataType = WgContext.GetIfAction (objectType);
			if (objectType == typeof (Delegate) || DataType != null)		// if Action<WgContext,T> or a generic delegate
			{
				Proc = WgContext.ActionForName (Info);
			}
			else
			{
				DataType = WgContext.GetIfPredicate (objectType);		// if predicate

				if (DataType != null)
				{
					Proc = WgContext.PredicateForName (Info);
				}
			}

			return Proc;
		}
	}
}
