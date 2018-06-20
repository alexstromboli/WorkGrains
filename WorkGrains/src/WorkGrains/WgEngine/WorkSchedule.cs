using System;

namespace WorkGrains
{
	public class WorkSchedule
	{
		public Guid? SignalId = null;
		public int UnixTime = 0;
		public bool Fork = false;

		public static WorkSchedule Immediate ()
		{
			return null;
		}

		public static WorkSchedule WaitTillUnix (int UnixTime)
		{
			return new WorkSchedule
				{
					UnixTime = UnixTime
				};
		}

		public static WorkSchedule WaitFor (int Seconds)
		{
			int CurrentTime = WgEngine.UnixTime ();
			int WaitTill = CurrentTime + Seconds;
			WorkSchedule Result = WaitTillUnix (WaitTill);
			return Result;
		}

		public static WorkSchedule WaitTill (DateTime dt)
		{
			int UnixTime = (int)(DateTime.UtcNow + (dt - DateTime.Now) - WgEngine.dt1970).TotalSeconds;
			WorkSchedule Result = WaitTillUnix (UnixTime);
			return Result;
		}

		public static WorkSchedule WaitTillUtc (DateTime dt)
		{
			int UnixTime = (int)(dt - WgEngine.dt1970).TotalSeconds;
			WorkSchedule Result = WaitTillUnix (UnixTime);
			return Result;
		}

		public static WorkSchedule AtSignal (bool Fork = false)
		{
			return new WorkSchedule
				{
					SignalId = Guid.NewGuid (),
					Fork = Fork
				};
		}
	}
}
