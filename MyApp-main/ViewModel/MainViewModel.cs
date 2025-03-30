using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyApp.ViewModel;

public partial class MainViewModel : BaseViewModel
{
    public ObservableCollection<AnimeCharacter> MyObservableList { get; } = [];
    JSONServices MyJSONService;
    CSVServices MyCSVServices;

    public MainViewModel(JSONServices MyJSONService, CSVServices MyCSVServices)
    {
        this.MyJSONService = MyJSONService;
        this.MyCSVServices = MyCSVServices;
    }
 
    [RelayCommand]
    internal async Task GoToDetails(string id)
    {
        IsBusy = true;

        await Shell.Current.GoToAsync("DetailsView", true, new Dictionary<string,object>
        {
            {"selectedAnimal",id}
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

        await MyJSONService.SetStrangeAnimals(Globals.MyAnimeCharacters);

        IsBusy = false;
    }
    
    internal async Task RefreshPage()
    {
        MyObservableList.Clear ();

        if(Globals.MyAnimeCharacters.Count == 0) Globals.MyAnimeCharacters = await MyJSONService.GetStrangeAnimals();

        foreach (var item in Globals.MyAnimeCharacters)
        {
            MyObservableList.Add(item);
        }
        var result = await MyJSONService.GetStrangeAnimals();

        System.Diagnostics.Debug.WriteLine("Résultat JSON récupéré : " + result.Count);
    }
}
