using Newtonsoft.Json;

namespace WorkGrains
{
	// code block's external data reference
	public class CodeBlockDataC
	{
		[JsonIgnore]
		public CodeBlockDataC OuterC;
	}

	// code block's external data reference, typified
	public class CodeBlockData<T> : CodeBlockDataC
		where T : CodeBlockDataC
	{
		[JsonIgnore]
		public T Outer
		{
			get { return (T)OuterC; }
		}
	}
}
