using System;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

	public interface IGrain
	{
		void Append (WgContext Context);
	}

	public class InnerBlockC
	{
		[JsonIgnore]
		public object OuterC;
	}

	public class InnerBlock<T> : InnerBlockC
	{
		[JsonIgnore]
		public T Outer
		{
			get { return (T)OuterC; }

			set { OuterC = value; }
		}
	}

	public class CallStackEntry
	{
		[JsonConverter (typeof (DelegateConverter))]
		public Delegate Proc;
		[JsonConverter (typeof (InnerBlockConverter))]
		public InnerBlockC Data;

		public static CallStackEntry MakeEmpty (InnerBlockC Data)
		{
			return new CallStackEntry { Data = Data, Proc = null };
		}

		public override string ToString ()
		{
			return $"{Proc?.Method.DeclaringType.Name ?? "null"}.{Proc?.Method.Name ?? "null"} - {Data?.GetType().Name ?? "null"}";
		}
	}

	public class For<T, F> : InnerBlock<T>, IGrain
		where F : For<T, F>, new ()
		where T : InnerBlockC
	{
		public Action<WgContext, F> Init;
		public Func<WgContext, F, bool> Check;
		public Action<WgContext, F> Step;
		public Action<WgContext, F> Body;
		public Action<WgContext, T> NextProc;

		public static IGrain Generate (
			Action<WgContext, F> Init,
			Func<WgContext, F, bool> Check,
			Action<WgContext, F> Step,
			Action<WgContext, F> Body,
			Action<WgContext, T> NextProc = null
			)
		{
			F This = new F ();
			This.Init = Init;
			This.Check = Check;
			This.Step = Step;
			This.Body = Body;
			This.NextProc = NextProc;

			return This;
		}

		public void Append (WgContext Context)
		{
			if (NextProc != null)
			{
				Context.ProceedTo (NextProc);
			}

			Context.ProceedTo (MakeStep, (F)this);

			if (Init != null)
			{
				Context.ProceedTo (Init);
			}
		}

		public static void MakeStep (WgContext Context, F Data)
		{
			bool Proceed = Data.Check (Context, Data);

			if (Proceed)
			{
				Context.ProceedTo (MakeStep, Data);

				if (Data.Step != null)
				{
					Context.ProceedTo (Data.Step);
				}

				if (Data.Body != null)
				{
					Context.ProceedTo (Data.Body);
				}
			}
		}
	}

	public class InnerBlockConverter : JsonConverter
	{
		public static readonly string TypePropName = "Type";
		public static readonly string DataPropName = "Data";

		public override bool CanConvert (Type objectType)
		{
			return objectType == typeof (InnerBlockC);
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			InnerBlockC Block = (InnerBlockC)value;
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
			InnerBlockC Data = DataToken.ToObject (DataType, serializer) as InnerBlockC;

			return Data;
		}
	}

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
			GrainInfo Info = WgContext.NameForDelegate ((Delegate)value, DataType);
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

	[JsonConverter (typeof (WgContextConverter))]
	public partial class WgContext
	{
		public Stack<CallStackEntry> CallStack = new Stack<CallStackEntry> ();

		public void Run (WaitHandle ehStop)
		{
			while (!ehStop.WaitOne (0) && CallStack.Count > 0)
			{
				CallStackEntry CurrentEntry = CallStack.Pop ();

				if (CurrentEntry.Proc == null)
				{
					continue;
				}

				// to keep data in stack
				if (CallStack.Count == 0
				    || !object.ReferenceEquals (CurrentEntry.Data, CallStack.Peek ().Data)
				    )
				{
					CallStack.Push (CallStackEntry.MakeEmpty (CurrentEntry.Data));
				}

				CurrentEntry.Proc.Method.Invoke (null, BindingFlags.Default, null, new[] { this, (object)CurrentEntry.Data }, Thread.CurrentThread.CurrentCulture);

				// DEBUG
				if ((DateTime.Now.Second % 10) > 6)
				{
					break;
				}
			}
		}

		//
		public void ProceedToGeneric (Delegate NextProc, InnerBlockC Data, Delegate FurtherProc, uint StartAt = 0)
		{
			InnerBlockC LastStackData = CallStack.Count == 0
				? null
				: CallStack.Peek ().Data
				;

			if (Data == null)
			{
				Data = LastStackData;
			}

			Data.OuterC = object.ReferenceEquals (Data, LastStackData)
				? LastStackData.OuterC
				: LastStackData
				;

			if (FurtherProc != null)
			{
				CallStack.Push (new CallStackEntry
					{
						Proc = FurtherProc,
						Data = LastStackData
					});
			}

			CallStack.Push (new CallStackEntry
				{
					Proc = NextProc,
					Data = Data
				});
		}

		//
		public void ProceedTo<T, F> (Action<WgContext, T> NextProc, T Data, Action<WgContext, F> FurtherProc, uint StartAt = 0)
			where T : InnerBlockC
		{
			ProceedToGeneric (NextProc, Data, FurtherProc, StartAt);
		}

		public void ProceedTo<T> (Action<WgContext, T> NextProc, T Data = default(T))
			where T : InnerBlockC
		{
			ProceedToGeneric (NextProc, Data, null, 0);
		}

		public void ProceedTo (IGrain Grain)
		{
			Grain.Append (this);
		}
	}
}
