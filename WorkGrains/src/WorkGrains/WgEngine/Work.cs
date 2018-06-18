using System;
using System.Collections.Generic;

using Newtonsoft.Json;

using WorkGrains.Converters;

namespace WorkGrains
{
	public class Work
	{
		public static readonly string DefaultLoopLabel = "";

		public Guid? SignalId;
		public int StartAt;

		// call stack
		[JsonConverter (typeof (CallStackEntryStackConverter))]
		public Stack<CallStackEntry> CallStack = new Stack<CallStackEntry> ();

		// 'break' or 'continue'
		[JsonProperty (NullValueHandling = NullValueHandling.Ignore)]
		public LeapInfo Leap;
	}
}
