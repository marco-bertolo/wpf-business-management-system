namespace PapJoaoSimoes.ViewModels
{
    public abstract class PageViewModel : ViewModelBase
    {
        protected PageViewModel(string title, string subtitle)
        {
            Title = title;
            Subtitle = subtitle;
        }

        public string Title { get; private set; }
        public string Subtitle { get; private set; }
    }
}
