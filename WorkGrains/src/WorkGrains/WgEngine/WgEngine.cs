using System;
using System.Threading;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace WorkGrains
{
    public class WgEngine
    {
		public List<Work> Works;

		[JsonIgnore]
		protected SortedDictionary<int, Work> ByStartAt;
		[JsonIgnore]
		protected SortedDictionary<Guid, Work> BySignalId;

		public void Run (WaitHandle whStop)
		{
			throw new NotImplementedException ();
		}

		public void StartWork (Delegate Proc, object Data, int StartAt = 0)
		{
			throw new NotImplementedException ();
		}

		public void ProcessSignal (Guid SignalId, object Data)
		{
			throw new NotImplementedException ();
		}
	}
}
