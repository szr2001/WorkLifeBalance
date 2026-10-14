namespace WorkLifeBalance.Shared.Data
{
    public static class StoredFormat
    {
        public const string Date = "MMddyyyy";
        public const string Time = "HHmmss";

        public static DateOnly ParseDate(string text) =>
            new(int.Parse(text.Substring(4, 4)), int.Parse(text.Substring(0, 2)), int.Parse(text.Substring(2, 2)));

        public static TimeOnly ParseTime(string text) =>
            new(int.Parse(text.Substring(0, 2)), int.Parse(text.Substring(2, 2)), int.Parse(text.Substring(4, 2)));
    }
}
