using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyApp.Model;
using MyApp.Service;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyApp.ViewModel;

public partial class UserListViewModel : BaseViewModel
{
    private readonly MongoUserService _userService;

    public ObservableCollection<User> Users { get; } = new();

    public UserListViewModel(MongoUserService userService)
    {
        _userService = userService;
    }

    [RelayCommand]
    public async Task LoadUsersAsync()
    {
        IsBusy = true;

        var userList = await _userService.GetAllUsersAsync(); // ✅ méthode async !
        Users.Clear();
        foreach (var user in userList)
            Users.Add(user);

        IsBusy = false;
    }
}
