﻿using CommunityToolkit.Maui.Views;
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
    [ObservableProperty]
    private string characterNameToDelete;

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

        try
        {
            MyScanner.OpenPort();
            MyScanner.SerialBuffer.Changed += OnSerialDataReception;
        }
        catch (Exception ex)
        {
            Shell.Current.DisplayAlert("Erreur de connexion", "Impossible d'accéder au port série. Le scanner est peut-être déconnecté.", "OK");
            System.Diagnostics.Debug.WriteLine($"❌ Erreur SerialPort : {ex.Message}");
        }

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
            MyScanner.SerialBuffer.Enqueue("3");
        }
    }

    [RelayCommand]
    internal async Task GoToDetails(string id)
    {
        if (Globals.CurrentUser == null || Globals.CurrentUser.Role?.ToLower() != "admin")
        {
            await Shell.Current.DisplayAlert(
                "Accès refusé",
                "Seuls les admins ont accès à cette fonctionalité.",
                "OK");
            return;
        }

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
        if (Globals.CurrentUser == null || Globals.CurrentUser.Role?.ToLower() != "admin")
        {
            await Shell.Current.DisplayAlert(
                "Accès refusé",
                "Seuls les admins peuvent exporter les données.",
                "OK");
            return;
        }

        IsBusy = true;

        await MyCSVServices.PrintData(Globals.MyAnimeCharacters);

        IsBusy = false;
    }

    [RelayCommand]
    internal async Task LoadFromCSV()
    {
        if (Globals.CurrentUser == null || Globals.CurrentUser.Role?.ToLower() != "admin")
        {
            await Shell.Current.DisplayAlert(
                "Accès refusé",
                "Seuls les admins peuvent importer des données.",
                "OK");
            return;
        }

        IsBusy = true;

        Globals.MyAnimeCharacters = await MyCSVServices.LoadData(Globals.MyAnimeCharacters);

        // 💾 Sauvegarde dans le JSON pour que ça persiste après fermeture
        await MyJSONService.SetAnimeCharacters(Globals.MyAnimeCharacters);

        await RefreshPage();

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

        System.Diagnostics.Debug.WriteLine($"👤 Utilisateur connecté : {Globals.CurrentUser?.Id}");

        if (Globals.CurrentUser == null)
        {
            await Shell.Current.GoToAsync("LoginView");
            return;
        }

        if (Globals.MyAnimeCharacters.Count == 0)
            Globals.MyAnimeCharacters = await MyJSONService.GetAnimeCharacters();

        var currentUserId = Globals.CurrentUser.Id.ToString();
        var userCharacters = Globals.MyAnimeCharacters
            .Where(c => c.UserIds != null && c.UserIds.Contains(currentUserId))
            .GroupBy(c => c.Id)
            .Select(g => g.First())
            .ToList();

        System.Diagnostics.Debug.WriteLine($"🧩 Personnages pour l'utilisateur {currentUserId} : {userCharacters.Count}");

        foreach (var item in userCharacters)
        {
            System.Diagnostics.Debug.WriteLine($"🔸 {item.Id} - {item.Name}");
            MyObservableList.Add(item);
        }


        System.Diagnostics.Debug.WriteLine("✅ Liste filtrée : " + MyObservableList.Count);
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
                        await GoToCharacterPreview(scannedId);
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

    [RelayCommand]
    async Task GoToCharacterCatalog()
    {
        await Shell.Current.GoToAsync("CharacterCatalogView");
    }

    [RelayCommand]
    async Task GoToCharacterPreview(string id)
    {
        await Shell.Current.GoToAsync(nameof(CharacterPreviewView), true, new Dictionary<string, object>
    {
        { "selectedCharacter", id }
    });
    }

    [RelayCommand]
    public async Task DeleteCharacterByName()
    {
        if (Globals.CurrentUser?.Role?.ToLower() != "admin")
        {
            await Shell.Current.DisplayAlert("Accès refusé", "Seuls les admins peuvent supprimer un personnage.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(CharacterNameToDelete))
        {
            await Shell.Current.DisplayAlert("Erreur", "Veuillez entrer un nom de personnage à supprimer.", "OK");
            return;
        }

        var list = await MyJSONService.GetAnimeCharacters();
        int countBefore = list.Count;

        list = list.Where(c => !string.Equals(c.Name, CharacterNameToDelete.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
        int countAfter = list.Count;

        if (countAfter < countBefore)
        {
            await MyJSONService.SetAnimeCharacters(list);
            System.Diagnostics.Debug.WriteLine($"🧹 {countBefore - countAfter} personnage(s) '{CharacterNameToDelete}' supprimé(s) du JSON.");
            await Shell.Current.DisplayAlert("Succès", $"Le personnage '{CharacterNameToDelete}' a été supprimé.", "OK");
            CharacterNameToDelete = string.Empty;
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"ℹ️ Aucun personnage nommé '{CharacterNameToDelete}' trouvé.");
            await Shell.Current.DisplayAlert("Info", $"Aucun personnage nommé '{CharacterNameToDelete}' n'a été trouvé.", "OK");
        }
    }

}
