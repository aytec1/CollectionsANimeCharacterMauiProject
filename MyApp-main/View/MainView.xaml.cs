using CommunityToolkit.Maui.Views;
using MyApp.Model;
using MyApp.ViewModel;

namespace MyApp.View;

public partial class MainView : ContentPage
{
    MainViewModel viewModel;

    private int tapCount = 0;
    private object? tappedSender;
    private IDispatcherTimer tapTimer;

    public MainView(MainViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;

        tapTimer = Dispatcher.CreateTimer();
        tapTimer.Interval = TimeSpan.FromMilliseconds(300); // temps pour détecter double clic
        tapTimer.Tick += OnTapTimerTick;
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

    public void OnCharacterTapped(object sender, TappedEventArgs e)
    {
        tappedSender = sender;
        tapCount++;

        if (!tapTimer.IsRunning)
            tapTimer.Start();
    }

    private void OnTapTimerTick(object? sender, EventArgs e)
    {
        tapTimer.Stop();

        if (tappedSender is VisualElement ve && ve.BindingContext is AnimeCharacter character)
        {
            if (tapCount == 1)
            {
                System.Diagnostics.Debug.WriteLine("Simple clic : GoToDetails");
                viewModel.GoToDetailsCommand.Execute(character.Id);
            }
            else if (tapCount >= 2)
            {
                System.Diagnostics.Debug.WriteLine(" Double clic : GoToCharacterPreview");
                viewModel.GoToCharacterPreviewCommand.Execute(character.Id);
            }
        }

        tapCount = 0;
        tappedSender = null;
    }
}
