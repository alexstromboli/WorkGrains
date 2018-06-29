using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Newtonsoft.Json;
using WorkGrains.Converters;

namespace WorkGrains
{
	public class WrongEngineStateException : Exception
	{
	}

	[JsonConverter (typeof(WgEngineConverter))]
	public partial class WgEngine : IDisposable
	{
		// get serialized
		public List<Work> Works;

		public string StateChangeToken = Guid.NewGuid ().ToString ("N");

		public enum EngineState
		{
			Stopping,
			Stopped,
			Running
		}

		protected EngineState State = EngineState.Stopped;

		protected class EngineRequest
		{
		}

		protected string WorkDirPath;

		protected ManualResetEvent mreStopping;     // set under lock RequestsQueue
		protected ManualResetEvent mreStopped;
		protected AutoResetEvent areRequestsEnqueued;

		protected SortedDictionary<int, Work> ByStartAt;
		protected SortedDictionary<Guid, Work> BySignalId;

		protected ConcurrentQueue<EngineRequest> RequestsQueue;

		public WgEngine ()
		{
		}

		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		public void Run (WaitHandle whStop, string WorkDirPath)
		{
			lock (StateChangeToken)
			{
				if (State != EngineState.Stopped)
				{
					throw new WrongEngineStateException ();
				}

				State = EngineState.Running;
			}

			//
			this.WorkDirPath = Path.GetFullPath (WorkDirPath);

			// here: lock pid file in the directory
			// check version

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

		protected void AppendRequest (EngineRequest Request)
		{
			lock (StateChangeToken)
			{
				if (State != EngineState.Running)
				{
					throw new WrongEngineStateException ();
				}

				RequestsQueue.Enqueue (Request);
				areRequestsEnqueued.Set ();
			}
		}
	}
}
