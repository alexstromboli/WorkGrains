using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _01_Items
{
	public class WgContextConverter : JsonConverter
	{
		public static readonly string StackPropName = "Stack";
		public static readonly string LeapPropName = "Leap";

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

			writer.WriteStartObject ();
			writer.WritePropertyName (StackPropName);
			serializer.Serialize (writer, Entries);

			if (Context.Leap != null)
			{
				writer.WritePropertyName (LeapPropName);
				serializer.Serialize (writer, Context.Leap);
			}

			writer.WriteEndObject ();
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JToken Raw = JToken.Load (reader);
			CallStackEntry[] Entries = Raw[StackPropName].ToObject<CallStackEntry[]> (serializer);

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

			//
			var RawLeap = Raw[LeapPropName];
			LeapInfo Leap = null;
			if (RawLeap != null)
			{
				Leap = RawLeap.ToObject<LeapInfo> (serializer);
			}

			//
			WgContext Context = new WgContext
				{
					CallStack = CallStack,
					Leap = Leap
				};

			return Context;
		}
	}
}
