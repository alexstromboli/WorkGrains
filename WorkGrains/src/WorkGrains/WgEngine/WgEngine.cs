using System;
using System.IO;
using System.Linq;
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
		public static readonly string TimeScheduleDirName = "time";
		public static readonly string SignalsDirName = "signals";

		// get serialized
		public List<Work> Works;

		public string StateChangeToken = Guid.NewGuid ().ToString ("N");

		public enum EngineState
		{
			Stopped,
			Running,
			Stopping,			// works are still running, but requests don't get processed, they get just stored for serialization
			Finalizing			// serialization of state
		}

		protected EngineState State = EngineState.Stopped;

		protected class EngineRequest
		{
		}

		// here: consider move to Run as local
		protected string WorkDirPath;
		protected string LockFilePath;
		protected string TimeScheduleDirPath;
		protected string SignalsDirPath;
		protected string NoWaitFilePath;
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
				TimeScheduleDirPath = Path.Combine (WorkDirPath, TimeScheduleDirName);
				SignalsDirPath = Path.Combine (WorkDirPath, SignalsDirName);
				NoWaitFilePath = Path.Combine (TimeScheduleDirPath, "0");

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
				areRequestsEnqueued = new AutoResetEvent (false);
				State = EngineState.Running;
			}

			// here: create mreStop

			// here: load requests queue
			// and delete the file
			// mark to-be-canceled ids

			// here: start all from "0" (and delete it)
			// flagging cancel for those requested cancel

			// main loop
			WaitHandle[] Wait = new WaitHandle[]
				{
					whStop,					// 0
					areRequestsEnqueued		// 1
				};

			while (true)
			{
				// here: process all requests

				// to wait by time
				int Now = UnixTime ();
				var Earliest = Directory.GetFiles (TimeScheduleDirPath, "*.*")
					.Select (p =>
						{
							string Name = Path.GetFileNameWithoutExtension (p);
							int Time;
							if (!int.TryParse (Name, out Time))
							{
								Time = -1;
							}

							return new
								{
									Path = p,
									Name = Path.GetFileNameWithoutExtension (p),
									Time
								};
						})
					.Where (e => e.Time != -1)
					.OrderBy (e => e.Time)
					.FirstOrDefault ()
					;

				string EarliestTimePath = Earliest?.Path;
				int StartByTime = Earliest?.Time ?? -1;

				Guid? StartByTimeId = EarliestTimePath == null
					? null
					: File.ReadAllLines (EarliestTimePath)
						.Select (s =>
								{
									Guid g;
									return Guid.TryParse (s, out g) ? g : (Guid?)null;
								})
							.FirstOrDefault (g => g != null)
					;

				int WaitForTime = StartByTimeId == null
						? Timeout.Infinite
						: StartByTime > Now
							? StartByTime - Now
							: 0
					;

				// wait
				int WaitIndex = StartByTimeId != null && WaitForTime == 0
					? WaitHandle.WaitTimeout
					: WaitHandle.WaitAny (Wait, WaitForTime)
					;

				if (WaitIndex == 0)     // stop
				{
					break;
				}

				if (WaitIndex == 1)     // request
				{
					continue;
				}

				// timeout, work to do
				if (StartByTimeId == null)      // can't be
				{
					continue;
				}

				// here: start work StartByTimeId
			}

			lock (StateChangeToken)
			{
				State = EngineState.Stopping;
			}

			// here: stop works

			lock (StateChangeToken)
			{
				State = EngineState.Finalizing;
			}

			// here: serialize requests

			// close on exit
			fsLock.Close ();

			// here: close all reset-events

			lock (StateChangeToken)
			{
				State = EngineState.Stopped;
			}

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
				if (State != EngineState.Running && State != EngineState.Stopping)
				{
					throw new WrongEngineStateException ();
				}

				RequestsQueue.Enqueue (Request);
				areRequestsEnqueued.Set ();
			}
		}
	}
}
