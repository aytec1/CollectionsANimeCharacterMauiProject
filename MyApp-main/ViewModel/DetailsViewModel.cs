using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Service;

namespace MyApp.ViewModel;

[QueryProperty(nameof(Id), "selectedAnimal")]
public partial class DetailsViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? Id { get; set; }

    [ObservableProperty]
    public partial string? Name { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }

    [ObservableProperty]
    public partial string? Picture { get; set; }

    [ObservableProperty]
    public partial string? Sound { get; set; }

    [ObservableProperty]
    public partial string? SpecialAttack { get; set; }

    [ObservableProperty]
    public partial string? Origin { get; set; }

    [ObservableProperty]
    public partial string? SerialBufferContent { get; set; }

    public List<string> OriginsList { get; } = new()
    {
        "One Piece",
        "Naruto",
        "Bleach",
        "Dragon Ball",
        "My Hero Academia",
        "Hunter x Hunter"
    };

    readonly JSONServices MyJSONService;

    public DetailsViewModel(JSONServices jsonService)
    {
        MyJSONService = jsonService;
    }

    internal void RefreshPage()
    {
        foreach (var item in Globals.MyAnimeCharacters)
        {
            if (Id == item.Id)
            {
                Name = item.Name;
                Description = item.Description;
                Picture = item.Picture;
                Sound = item.Sound;
                SpecialAttack = item.SpecialAttack;
                Origin = item.Origin;
                break;
            }
        }
    }

    internal void ClosePage()
    {
        // plus de port à fermer ici car plus de scanner ici
    }

    [RelayCommand]
    internal async Task ChangeObjectParametersAsync()
    {
        var currentUserId = Globals.CurrentUser.Id.ToString();

        if (string.IsNullOrWhiteSpace(Id))
        {
            await Shell.Current.DisplayAlert("Erreur", "L'identifiant ne peut pas être vide.", "OK");
            return;
        }

        if (!Id.All(char.IsDigit))
        {
            await Shell.Current.DisplayAlert("Erreur", "L'identifiant ne peut contenir que des chiffres.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(Description) ||
            string.IsNullOrWhiteSpace(Picture) ||
            string.IsNullOrWhiteSpace(Sound) ||
            string.IsNullOrWhiteSpace(SpecialAttack) ||
            string.IsNullOrWhiteSpace(Origin))
        {
            await Shell.Current.DisplayAlert("Erreur",
                "Tous les champs sont obligatoires : Nom, Description, Image, Son, Attaque spéciale, Origine.",
                "OK");
            return;
        }

        var existing = Globals.MyAnimeCharacters.FirstOrDefault(x => x.Id == Id);

        if (existing != null)
        {
            // 🔒 Vérifie si on tente de créer un personnage avec le même Id mais un contenu différent
            if (existing.Name != Name || existing.Origin != Origin)
            {
                await Shell.Current.DisplayAlert("Erreur",
                    "Un personnage avec cet ID existe déjà avec un contenu différent.",
                    "OK");
                return;
            }

            // ✅ Mise à jour si utilisateur pas encore lié
            existing.Name = Name!;
            existing.Description = Description!;
            existing.Picture = Picture!;
            existing.Sound = Sound!;
            existing.SpecialAttack = SpecialAttack!;
            existing.Origin = Origin!;

            if (!existing.UserIds.Contains(currentUserId))
                existing.UserIds.Add(currentUserId);
        }
        else
        {
            // ✅ Création autorisée uniquement si aucun autre personnage avec ce même ID n'existe
            Globals.MyAnimeCharacters.Add(new AnimeCharacter
            {
                Id = Id!,
                Name = Name!,
                Description = Description!,
                Picture = Picture!,
                Sound = Sound!,
                SpecialAttack = SpecialAttack!,
                Origin = Origin!,
                UserIds = new List<string> { currentUserId }
            });
        }

        await MyJSONService.SetAnimeCharacters(Globals.MyAnimeCharacters);
    }

}
