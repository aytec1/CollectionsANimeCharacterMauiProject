using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyApp.Model;
using MyApp.Service;
using MongoDB.Driver;


namespace MyApp.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    public ObservableCollection<AnimeCharacter> MyObservableList { get; } = [];
    JSONServices MyJSONService;
    CSVServices MyCSVServices;
    DeviceOrientationService MyScanner;
    IDispatcherTimer Emulator = Application.Current.Dispatcher.CreateTimer();

    [ObservableProperty]
    private bool emulatorOnOff = false;

    public bool isNavigating = false; // ✅ Pour éviter les redirections multiples
    private bool hasScanned = false; // ✅ Pour éviter la répétition du même ID

    partial void OnEmulatorOnOffChanged(bool value)
    {
        if (value)
        {
            System.Diagnostics.Debug.WriteLine("🚀 Emulateur activé");
            Emulator.Start();
        }
        else
        {
            Emulator.Stop();
            System.Diagnostics.Debug.WriteLine("🛑 Emulateur arrêté");
        }
    }

    public MainViewModel(JSONServices MyJSONService, CSVServices MyCSVServices, DeviceOrientationService myScanner)
    {
        this.MyJSONService = MyJSONService;
        this.MyCSVServices = MyCSVServices;
        this.MyScanner = myScanner;

        MyScanner.OpenPort();
        MyScanner.SerialBuffer.Changed += OnSerialDataReception;

        // Dès qu’un vrai scanner est branché, on coupe l’émulateur
        EmulatorOnOff = false;

        Emulator.Interval = TimeSpan.FromSeconds(1);
        Emulator.Tick += (s, e) => SimulateScan();
    }

    private void SimulateScan()
    {
        if (!hasScanned)
        {
            hasScanned = true;
            MyScanner.SerialBuffer.Enqueue("4");
        }
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
    async Task GoToUserCreation()
    {
        await Shell.Current.GoToAsync("UserCreationView");
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

        Globals.MyAnimeCharacters = await MyCSVServices.LoadData(Globals.MyAnimeCharacters);


        IsBusy = false;
    }

    [RelayCommand]
    internal async Task CleanAndSave()
    {
        IsBusy = true;

        // Supprimer les personnages sans ID
        Globals.MyAnimeCharacters = Globals.MyAnimeCharacters
            .Where(c => !string.IsNullOrWhiteSpace(c.Id))
            .ToList();

        // Réécriture du JSON propre
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
        if (isNavigating) return; // ✅ Évite les appels multiples

        // ✅ Dès qu’un vrai scan arrive, on arrête l’émulateur
        if (EmulatorOnOff)
        {
            EmulatorOnOff = false;
        }

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
                    isNavigating = true;
                    hasScanned = false; // ✅ On autorise un nouveau scan plus tard
                    System.Diagnostics.Debug.WriteLine("✅ Personnage trouvé. Redirection...");

                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await GoToDetails(scannedId);
                        isNavigating = false;
                    });
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

    [RelayCommand]
    async Task GoToUserList()
    {
        await Shell.Current.GoToAsync("UserListView");
    }

    [RelayCommand]
    async Task GoToLogin()
    {
        await Shell.Current.GoToAsync("LoginView");
    }

}
