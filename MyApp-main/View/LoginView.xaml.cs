using MyApp.ViewModel;

namespace MyApp.View;

public partial class LoginView : ContentPage
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void LoginButton_Pressed(object sender, EventArgs e)
    {
        if (sender is Button button)
            button.BackgroundColor = Colors.DarkRed; // Changement de couleur au clic
    }

    private void LoginButton_Released(object sender, EventArgs e)
    {
        if (sender is Button button)
            button.BackgroundColor = Color.FromArgb("#E74C3C"); // Retour à la couleur normale
    }

    private void CreateAccountButton_Pressed(object sender, EventArgs e)
    {
        if (sender is Button button)
            button.BackgroundColor = Colors.LightGray; // Couleur temporaire pour effet visuel
    }

    private void CreateAccountButton_Released(object sender, EventArgs e)
    {
        if (sender is Button button)
            button.BackgroundColor = Colors.Transparent; // Retour à transparent
    }
}
