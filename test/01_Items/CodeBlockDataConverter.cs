using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _01_Items
{
	// stack entry (de-)serializer
	public class CodeBlockDataConverter : JsonConverter
	{
		public static readonly string TypePropName = "Type";
		public static readonly string DataPropName = "Data";

		public override bool CanConvert (Type objectType)
		{
			return objectType == typeof (CodeBlockDataC);
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			CodeBlockDataC Block = (CodeBlockDataC)value;
			writer.WriteStartObject ();
			writer.WritePropertyName (TypePropName);
			serializer.Serialize (writer, Block.GetType ());
			writer.WritePropertyName (DataPropName);
			serializer.Serialize (writer, Block);
			writer.WriteEndObject ();
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
			{
				return null;
			}

			JObject Obj = JObject.Load (reader);
			Type DataType = Obj[TypePropName].ToObject<Type> ();

			JToken DataToken = Obj[DataPropName];
			CodeBlockDataC Data = DataToken.ToObject (DataType, serializer) as CodeBlockDataC;

			return Data;
		}
	}
}
