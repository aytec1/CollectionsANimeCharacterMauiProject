using AnimeDex.ViewModels;

namespace AnimeDex
{
    public partial class MainPage : ContentPage
    {
        private MainViewModel _viewModel;

        public MainPage()
        {
            InitializeComponent();

            // Crée et assigne le ViewModel à la page
            _viewModel = new MainViewModel();
            this.BindingContext = _viewModel;
        }

        private async void OnAddCharacterClicked(object sender, EventArgs e)
        {
            // Navigation vers la page d’ajout en lui passant le ViewModel
            await Navigation.PushAsync(new AddCharacterPage(_viewModel));
        }
    }
}
