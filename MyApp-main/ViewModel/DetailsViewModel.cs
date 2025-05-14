using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Service; // 👈 Ajoute cette ligne pour accéder à JSONServices

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

    public List<string> OriginsList { get; } = new() // 🆕 Liste pour le Picker
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
        var existing = Globals.MyAnimeCharacters.FirstOrDefault(x => x.Id == Id);

        if (existing != null)
        {
            // ✏️ Mise à jour d’un personnage existant
            existing.Name = Name ?? string.Empty;
            existing.Description = Description ?? string.Empty;
            existing.Picture = Picture ?? string.Empty;
            existing.Sound = Sound ?? string.Empty;
            existing.SpecialAttack = SpecialAttack ?? string.Empty;
            existing.Origin = Origin ?? string.Empty;
        }
        else if (!string.IsNullOrWhiteSpace(Id))
        {
            // ➕ Ajout d’un nouveau personnage
            Globals.MyAnimeCharacters.Add(new AnimeCharacter
            {
                Id = Id,
                Name = Name ?? string.Empty,
                Description = Description ?? string.Empty,
                Picture = Picture ?? string.Empty,
                Sound = Sound ?? string.Empty,
                SpecialAttack = SpecialAttack ?? string.Empty,
                Origin = Origin ?? string.Empty,
                UserId = Globals.CurrentUser.Id.ToString()
            });
        }

        //  Sauvegarde dans le JSON distant
        await MyJSONService.SetAnimeCharacters(Globals.MyAnimeCharacters);
    }


}
