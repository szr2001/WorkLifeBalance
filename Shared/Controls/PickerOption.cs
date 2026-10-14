namespace WorkLifeBalance.Shared.Controls
{
    public record PickerOption(object Value, string Label)
    {
        // 0..n as options, with an optional label for the 0 ("Any")
        public static IReadOnlyList<PickerOption> Numbers(IEnumerable<int> values, string? zeroLabel = null) =>
            values.Select(value => new PickerOption(value, value == 0 && zeroLabel != null ? zeroLabel : value.ToString())).ToList();
    }
}
