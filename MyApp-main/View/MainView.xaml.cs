using CommunityToolkit.Maui.Views;

namespace MyApp.View;

public partial class MainView : ContentPage
{
	MainViewModel viewModel;
	public MainView(MainViewModel viewModel)
	{
		this.viewModel = viewModel;
		InitializeComponent();
		BindingContext=viewModel;
	}
	protected override async void OnNavigatedTo(NavigatedToEventArgs args)
	{
        base.OnNavigatedTo(args);

        BindingContext = null;
		await viewModel.RefreshPage();    
		BindingContext = viewModel;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Globals.CurrentUser == null)
        {
            Shell.Current.GoToAsync("//LoginView");
        }
    }

}