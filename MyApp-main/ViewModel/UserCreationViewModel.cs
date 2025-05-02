using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Model;
using MyApp.Service;
using System.Data;
using System.Threading.Tasks;

namespace MyApp.ViewModel;

public partial class UserCreationViewModel : ObservableObject
{
    private MongoUserService mongoService = new MongoUserService();

    [ObservableProperty] private string firstName;
    [ObservableProperty] private string lastName;
    [ObservableProperty] private string email;
    [ObservableProperty] private string password;
    [ObservableProperty] private string role;

    [RelayCommand]
    private async Task CreateUser()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await App.Current.MainPage.DisplayAlert("Erreur", "Email et mot de passe obligatoires.", "OK");
            return;
        }

        var user = new User
        {
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            Password = Password,
            Role = Role
        };

        mongoService.AddUser(user);

        await App.Current.MainPage.DisplayAlert("Succès", "Utilisateur créé !", "OK");
        ClearFields();
    }

    private void ClearFields()
    {
        FirstName = LastName = Email = Password = Role = string.Empty;
    }
}
