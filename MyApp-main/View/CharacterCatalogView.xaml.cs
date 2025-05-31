namespace MyApp.View;

public partial class CharacterCatalogView : ContentPage
{
    public CharacterCatalogView()
	{
		InitializeComponent();
	}

    private void AddToCollectionButton_Pressed(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Colors.DarkOrange;
        }
    }

    private void AddToCollectionButton_Released(object sender, EventArgs e)
    {
        if (sender is Button button)
        {
            button.BackgroundColor = Color.FromArgb("#E67E22");
        }
    }
}