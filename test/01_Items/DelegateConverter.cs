using System;

using Newtonsoft.Json;

namespace _01_Items
{
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
			if (objectType == typeof (Delegate) || DataType != null)
			{
				Proc = WgContext.ActionForName (Info);
			}
			else
			{
				DataType = WgContext.GetIfPredicate (objectType);

				if (DataType != null)
				{
					Proc = WgContext.PredicateForName (Info);
				}
			}

			return Proc;
		}
	}
}
