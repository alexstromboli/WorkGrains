using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WorkGrains.Converters
{
	public class WgEngineConverter : JsonConverter
	{
		public override bool CanConvert (Type objectType)
		{
			return objectType == typeof (WgEngine);
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			WgEngine Engine = (WgEngine)value;
			writer.WriteStartObject ();
			writer.WritePropertyName (nameof (WgEngine.Works));
			serializer.Serialize (writer, Engine.Works);
			writer.WriteEndObject ();
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// here: replace JObject.Load with picking and deserializing due property
			JObject Obj = JObject.Load (reader);
			List<Work> Works = Obj[nameof (WgEngine.Works)].ToObject<List<Work>> ();

			WgEngine Engine = new WgEngine { Works = Works};

			return Engine;
		}
	}
}
