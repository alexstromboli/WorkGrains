using System;

using Newtonsoft.Json;

using WorkGrains.Converters;

namespace WorkGrains
{
	public class CallStackEntry
	{
		[JsonConverter (typeof (DelegateConverter))]
		public Delegate Proc;
		[JsonConverter (typeof (CodeBlockDataConverter))]
		public CodeBlockDataC Data;
		[JsonProperty (NullValueHandling = NullValueHandling.Ignore)]
		public string LoopHeader;

		public static CallStackEntry MakeEmpty (CodeBlockDataC Data)
		{
			return new CallStackEntry { Data = Data, Proc = null };
		}

		public override string ToString ()
		{
			return $"{Proc?.Method.DeclaringType.Name ?? "null"}.{Proc?.Method.Name ?? "null"} - {Data?.GetType().Name ?? "null"}";
		}
	}
}
