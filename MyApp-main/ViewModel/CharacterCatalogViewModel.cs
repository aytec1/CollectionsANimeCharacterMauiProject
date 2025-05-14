using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;

namespace MyApp.ViewModel 
{
    public partial class CharacterCatalogViewModel : ObservableObject
    {
        public ObservableCollection<AnimeCharacter> AllCharacters { get; } = new();
        private readonly JSONServices _jsonService = new();

        public CharacterCatalogViewModel()
        {
            LoadCharacters();
        }

        private async void LoadCharacters()
        {
            var list = await _jsonService.GetAnimeCharacters();
            var unique = list
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .ToList();

            AllCharacters.Clear();
            foreach (var c in unique)
                AllCharacters.Add(c);
        }

        [RelayCommand]
        private async Task AddToUserCollection(AnimeCharacter character)
        {
            if (Globals.CurrentUser == null)
            {
                await Shell.Current.DisplayAlert("Erreur", "Veuillez vous connecter.", "OK");
                return;
            }

            var alreadyExists = Globals.MyAnimeCharacters.Any(c =>
                c.Id == character.Id && c.UserId == Globals.CurrentUser.Id.ToString());

            if (alreadyExists)
            {
                await Shell.Current.DisplayAlert("Info", "Déjà dans votre collection.", "OK");
                return;
            }

            var newChar = new AnimeCharacter
            {
                Id = character.Id,
                Name = character.Name,
                Description = character.Description,
                Picture = character.Picture,
                Sound = character.Sound,
                SpecialAttack = character.SpecialAttack,
                Origin = character.Origin,
                UserId = Globals.CurrentUser.Id.ToString()
            };

            Globals.MyAnimeCharacters.Add(newChar);
            await _jsonService.SetAnimeCharacters(Globals.MyAnimeCharacters);

            await Shell.Current.DisplayAlert("Succès", "Ajouté à votre collection.", "OK");
        }
    }
}


