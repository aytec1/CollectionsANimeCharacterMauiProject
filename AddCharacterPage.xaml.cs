using AnimeDex.Models;
using AnimeDex.ViewModels;

namespace AnimeDex;

public partial class AddCharacterPage : ContentPage
{
    private MainViewModel _viewModel;

    public AddCharacterPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        // V�rification simple
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            await DisplayAlert("Erreur", "Le nom est obligatoire", "OK");
            return;
        }

        var character = new AnimeCharacter
        {
            Name = NameEntry.Text,
            Description = DescriptionEditor.Text,
            ImagePath = ImagePathEntry.Text,
            Strength = int.TryParse(StrengthEntry.Text, out var str) ? str : 0,
            Intelligence = int.TryParse(IntelligenceEntry.Text, out var intel) ? intel : 0
        };

        _viewModel.AddCharacter(character);

        await DisplayAlert("Ajout�", "Personnage ajout� avec succ�s", "OK");
        await Navigation.PopAsync(); // Retour � la page principale
    }
}
