using CommunityToolkit.Maui.Views;
using MyApp.ViewModel;

namespace MyApp.View;

public partial class UserCreationView : ContentPage
{
    public UserCreationView()
    {
        InitializeComponent();
        this.BindingContext = new UserCreationViewModel(); 
    }
    private void CreateUserButton_Pressed(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Colors.DarkRed; // Effet press�
        }
    }

    private void CreateUserButton_Released(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromArgb("#E74C3C"); // Retour � la couleur normale
        }
    }
}
