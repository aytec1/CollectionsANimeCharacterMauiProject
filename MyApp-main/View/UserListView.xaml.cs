using MyApp.ViewModel;

namespace MyApp.View;

public partial class UserListView : ContentPage
{
    public UserListView(UserListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadUsersAsync();
    }
}
