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

    [ObservableProperty]
    public partial bool EmulatorON_OFF { get; set; } = false;

    public List<string> OriginsList { get; } = new() // 🆕 Liste pour le Picker
    {
        "One Piece",
        "Naruto",
        "Bleach",
        "Dragon Ball",
        "My Hero Academia",
        "Hunter x Hunter"
    };

    readonly DeviceOrientationService MyScanner;
    readonly JSONServices MyJSONService;
    IDispatcherTimer emulator = Application.Current.Dispatcher.CreateTimer();

    public DetailsViewModel(DeviceOrientationService myScanner, JSONServices jsonService)
    {
        MyScanner = myScanner;
        MyJSONService = jsonService;

        MyScanner.OpenPort();
        myScanner.SerialBuffer.Changed += OnSerialDataReception;

        emulator.Interval = TimeSpan.FromSeconds(1);
        emulator.Tick += (s, e) => AddCode();
    }

    partial void OnEmulatorON_OFFChanged(bool value)
    {
        if (value) emulator.Start();
        else emulator.Stop();
    }

    private void AddCode()
    {
        MyScanner.SerialBuffer.Enqueue("B");
    }

    private void OnSerialDataReception(object sender, EventArgs arg)
    {
        DeviceOrientationService.QueueBuffer MyLocalBuffer = (DeviceOrientationService.QueueBuffer)sender;

        if (MyLocalBuffer.Count > 0)
        {
            SerialBufferContent += MyLocalBuffer.Dequeue().ToString();
            OnPropertyChanged(nameof(SerialBufferContent));
        }
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
        MyScanner.SerialBuffer.Changed -= OnSerialDataReception;
        MyScanner.ClosePort();
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
                Origin = Origin ?? string.Empty
            });
        }

        //  Sauvegarde dans le JSON distant
        await MyJSONService.SetAnimeCharacters(Globals.MyAnimeCharacters);
    }
}
