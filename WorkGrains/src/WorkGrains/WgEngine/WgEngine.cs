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
		public static readonly string EngineVersionToken = "1";
		public static readonly string EngineLockFileName = "lock";

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

		// here: consider move to Run as local
		protected string WorkDirPath;
		protected string LockFilePath;
		protected FileStream fsLock;

		protected ManualResetEvent mreStopping;     // set under lock StateChangeToken
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

				//
				this.WorkDirPath = Path.GetFullPath (WorkDirPath);
				LockFilePath = Path.Combine (WorkDirPath, EngineLockFileName);

				// lock key file in the directory
				try
				{
					if (File.Exists (LockFilePath))
					{
						fsLock = new FileStream (LockFilePath, FileMode.CreateNew, FileAccess.Read, FileShare.None);

						// check version
						string Version;
						using (StreamReader rdr = new StreamReader (fsLock))
						{
							Version = rdr.ReadToEnd ().Trim ();
						}

						if (Version != EngineVersionToken)
						{
							// here: throw due
							throw new Exception ();
						}
					}
					else
					{
						Directory.CreateDirectory (this.WorkDirPath);
						fsLock = new FileStream (LockFilePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
						using (StreamWriter wr = new StreamWriter (fsLock))
						{
							wr.Write (EngineVersionToken);
						}
					}
				}
				catch (Exception ex)
				{
					// here: handle failure to lock file
				}

				//
				State = EngineState.Running;
			}

			// close on exit
			fsLock.Close ();

			//
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
