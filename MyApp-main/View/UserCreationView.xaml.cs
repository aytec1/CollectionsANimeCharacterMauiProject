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
}
