using MyApp.ViewModel;

namespace MyApp.View;

public partial class CharacterPreviewView : ContentPage
{
    private readonly CharacterPreviewViewModel _viewModel;

    public CharacterPreviewView()
    {
        InitializeComponent();
        _viewModel = new CharacterPreviewViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.StopSound(); 
    }
}
