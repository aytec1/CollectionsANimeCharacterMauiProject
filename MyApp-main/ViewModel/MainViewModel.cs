using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Model;
using MyApp.Service; 

namespace MyApp.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    public ObservableCollection<AnimeCharacter> MyObservableList { get; } = [];
    JSONServices MyJSONService;
    CSVServices MyCSVServices;
    DeviceOrientationService MyScanner; 

    public MainViewModel(JSONServices MyJSONService, CSVServices MyCSVServices, DeviceOrientationService myScanner)
    {
        this.MyJSONService = MyJSONService;
        this.MyCSVServices = MyCSVServices;
        this.MyScanner = myScanner;

        MyScanner.OpenPort();
        MyScanner.SerialBuffer.Changed += OnSerialDataReception;
    }

    [RelayCommand]
    internal async Task GoToDetails(string id)
    {
        IsBusy = true;

        await Shell.Current.GoToAsync("DetailsView", true, new Dictionary<string, object>
        {
            {"selectedAnimal", id}
        });

        IsBusy = false;
    }

    [RelayCommand]
    internal async Task GoToGraph()
    {
        IsBusy = true;

        await Shell.Current.GoToAsync("GraphView", true);

        IsBusy = false;
    }

    [RelayCommand]
    internal async Task PrintToCSV()
    {
        IsBusy = true;

        await MyCSVServices.PrintData(Globals.MyAnimeCharacters);

        IsBusy = false;
    }

    [RelayCommand]
    internal async Task LoadFromCSV()
    {
        IsBusy = true;

        Globals.MyAnimeCharacters = await MyCSVServices.LoadData();

        IsBusy = false;
    }

    [RelayCommand]
    internal async Task UploadJSON()
    {
        IsBusy = true;

        // 🔥 Ajout temporaire d’un personnage si la liste est vide
        if (Globals.MyAnimeCharacters.Count == 0)
        {
            Globals.MyAnimeCharacters.Add(new AnimeCharacter
            {
                Id = "1",
                Name = "luffy",
                Description = "Capitaine chapeau de paille",
                Picture = "luffy.png",
                SpecialAttack = "Gomu Gomu no Bazooka",
                Sound = "luffy.mp3"
            });
        }

        await MyJSONService.SetAnimeCharacters(Globals.MyAnimeCharacters);

        IsBusy = false;
    }

    internal async Task RefreshPage()
    {
        MyObservableList.Clear();

        if (Globals.MyAnimeCharacters.Count == 0)
            Globals.MyAnimeCharacters = await MyJSONService.GetAnimeCharacters();

        foreach (var item in Globals.MyAnimeCharacters)
        {
            MyObservableList.Add(item);
        }
        var result = await MyJSONService.GetAnimeCharacters();

        System.Diagnostics.Debug.WriteLine("Résultat JSON récupéré : " + result.Count);
    }

    private async void OnSerialDataReception(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("🛰️ Donnée reçue dans le buffer.");

        if (MyScanner.SerialBuffer.Count > 0)
        {
            var scannedId = MyScanner.SerialBuffer.Dequeue()?.ToString().Trim();
            System.Diagnostics.Debug.WriteLine($"🔍 QR scanné : {scannedId}");

            if (!string.IsNullOrWhiteSpace(scannedId))
            {
                var character = Globals.MyAnimeCharacters.FirstOrDefault(c => c.Id == scannedId);
                if (character != null)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Personnage trouvé. Redirection...");
                    await GoToDetails(scannedId);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ Aucun personnage trouvé avec cet ID.");
                    foreach (var item in Globals.MyAnimeCharacters)
                    {
                        System.Diagnostics.Debug.WriteLine($"🔸 Id: {item.Id}, Nom: {item.Name}, Description: {item.Description}");
                    }
                }
            }
        }
    }

}
