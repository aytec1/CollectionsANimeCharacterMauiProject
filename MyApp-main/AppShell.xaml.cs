using MyApp.View;


namespace MyApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(DetailsView), typeof(DetailsView));
            Routing.RegisterRoute(nameof(GraphView), typeof(GraphView));
            Routing.RegisterRoute(nameof(UserCreationView), typeof(UserCreationView));
            Routing.RegisterRoute(nameof(UserListView), typeof(UserListView));
            Routing.RegisterRoute(nameof(LoginView), typeof(LoginView));
            Routing.RegisterRoute(nameof(CharacterCatalogView), typeof(CharacterCatalogView));
            Routing.RegisterRoute(nameof(MainView), typeof(MainView));
            Routing.RegisterRoute(nameof(CharacterPreviewView), typeof(CharacterPreviewView));
        }
    }
}
