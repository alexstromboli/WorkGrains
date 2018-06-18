using System;

namespace WorkGrains
{
	// exception to force 'break' or 'continue'
	public class WgLoopException : Exception
	{
		public LeapInfo LeapInfo;

		public WgLoopException (LeapInfo.LeapType Type, string LoopHeader)
		{
			LeapInfo = new LeapInfo
				{
					Type = Type,
					LoopHeader = LoopHeader ?? WgContext.DefaultLoopLabel
				};
		}
	}
}
