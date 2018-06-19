using System;

namespace WorkGrains
{
	public partial class WgEngine
	{
		public static readonly DateTime dt1970 = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static int UnixTime ()
		{
			return (int)(DateTime.UtcNow - dt1970).TotalSeconds;
		}
	}
}
