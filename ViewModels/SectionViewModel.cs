namespace PapJoaoSimoes.ViewModels
{
    public class SectionViewModel : PageViewModel
    {
        public SectionViewModel(
            string title,
            string subtitle,
            string emptyMessage,
            string columnOne,
            string columnTwo,
            string columnThree,
            string columnFour)
            : base(title, subtitle)
        {
            EmptyMessage = emptyMessage;
            ColumnOne = columnOne.ToUpperInvariant();
            ColumnTwo = columnTwo.ToUpperInvariant();
            ColumnThree = columnThree.ToUpperInvariant();
            ColumnFour = columnFour.ToUpperInvariant();
        }

        public string EmptyMessage { get; private set; }
        public string ColumnOne { get; private set; }
        public string ColumnTwo { get; private set; }
        public string ColumnThree { get; private set; }
        public string ColumnFour { get; private set; }
    }
}
