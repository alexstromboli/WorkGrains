namespace WorkGrains
{
	// stackable object (loop, code block, etc.)
	public interface IGrain
	{
		void Append (WgContext Context);
	}
}
