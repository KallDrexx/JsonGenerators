namespace JsonGenerators
{
    public readonly struct JsonSection
    {
        public readonly int StartIndex { get; }
        public readonly int EndIndex { get; }
        public readonly int? NextCharIndex { get; }

        public JsonSection(int startIndex, int endIndex, int? nextCharIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
            NextCharIndex = nextCharIndex;
        }
    }
}