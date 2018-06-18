namespace WorkGrains
{
	// 'break' or 'continue'
	public class LeapInfo
	{
		public enum LeapType
		{
			Break = 1,
			Continue
		}

		public LeapType Type;
		public string LoopHeader;
	}
}
