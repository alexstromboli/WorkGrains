using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace _01_Items
{
	public class WgContextConverter : JsonConverter
	{
		public override bool CanConvert (Type objectType)
		{
			return objectType == typeof (WgContext);
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			WgContext Context = (WgContext)value;
			CallStackEntry[] Entries = Context.CallStack.ToArray ();

			for (int i = 0; i < Entries.Length - 1; ++i)
			{
				if (object.ReferenceEquals (Entries[i].Data, Entries[i + 1].Data))
				{
					Entries[i].Data = null;
				}
			}

			serializer.Serialize (writer, Entries);
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			CallStackEntry[] Entries = serializer.Deserialize<CallStackEntry[]> (reader);

			for (int i = Entries.Length - 2; i >= 0; --i)
			{
				if (Entries[i].Data == null)
				{
					Entries[i].Data = Entries[i + 1].Data;
				}
				else
				{
					Entries[i].Data.OuterC = Entries[i + 1].Data;
				}
			}

			Stack<CallStackEntry> CallStack = new Stack<CallStackEntry> (Entries.Reverse ());

			WgContext Context = new WgContext { CallStack = CallStack };

			return Context;
		}
	}
}
