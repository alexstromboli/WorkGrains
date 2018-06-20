using System;
using System.Threading;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace WorkGrains
{
	public partial class WgEngine : IDisposable
	{
		public List<Work> Works;

		[JsonIgnore]
		protected SortedDictionary<int, Work> ByStartAt;
		[JsonIgnore]
		protected SortedDictionary<Guid, Work> BySignalId;

		public WgEngine (string WorkDirPath)
		{
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		public void Run (WaitHandle whStop)
		{
			throw new NotImplementedException ();
		}

		public void StartWork (Delegate Proc, object Data, WorkSchedule StartAt = null)
		{
			throw new NotImplementedException ();
		}

		public void ProcessSignal (Guid SignalId, object Data)
		{
			throw new NotImplementedException ();
		}
	}
}
