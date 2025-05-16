using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Service;
using MyApp.Model;
using System.Linq;
using System.Threading.Tasks;

namespace MyApp.ViewModel;

public partial class LoginViewModel : ObservableObject
{
    private readonly MongoUserService _userService;

    public LoginViewModel(MongoUserService userService)
    {
        _userService = userService;
    }

    [ObservableProperty]
    private string email;

    [ObservableProperty]
    private string password;

    [ObservableProperty]
    private string errorMessage;

    [ObservableProperty]
    private bool hasError;

    [RelayCommand]
    private async Task LoginAsync()
    {
        HasError = false;
        System.Diagnostics.Debug.WriteLine($"🔐 Tentative de connexion avec : {Email} / {Password}");

        try
        {
            var users = await _userService.GetAllUsersAsync();
            System.Diagnostics.Debug.WriteLine($"📦 {users.Count} utilisateurs récupérés");

            var user = users.FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine("✅ Connexion réussie");
                Globals.CurrentUser = user;
                await Shell.Current.GoToAsync("//MainView");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("❌ Identifiants incorrects");
                ErrorMessage = "Email ou mot de passe incorrect.";
                HasError = true;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("🚨 ERREUR : " + ex.Message);
            ErrorMessage = "Une erreur est survenue : " + ex.Message;
            HasError = true;
        }
    }
    [RelayCommand]
    async Task GoToUserCreation()
    {
        await Shell.Current.GoToAsync("UserCreationView");
    }


}
