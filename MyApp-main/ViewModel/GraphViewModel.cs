using Microcharts;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using MyApp.Model; // Pour accéder à AnimeCharacter
using MyApp.Service; // Si tu as besoin de JSONServices
using System.Threading.Tasks;

namespace MyApp.ViewModel;

public partial class GraphViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? CharacterOrigin { get; set; }

    [ObservableProperty]
    public partial Chart MyObservableChart { get; set; } = new PieChart();

    public GraphViewModel()
    {
        LoadPieChartFromCharacters();
    }

    private void LoadPieChartFromCharacters()
    {
        // Récupère et regroupe les personnages par origine
        var groupedByOrigin = Globals.MyAnimeCharacters
            .Where(c => !string.IsNullOrEmpty(c.Origin))
            .GroupBy(c => c.Origin)
            .Select(g => new { Origin = g.Key, Count = g.Count() })
            .ToList();

        // Crée les entrées du graphique
        var entries = groupedByOrigin.Select(g => new ChartEntry(g.Count)
        {
            Label = g.Origin,
            ValueLabel = g.Count.ToString(),
            Color = SKColor.Parse(GetRandomColorHex())
        }).ToArray();

        MyObservableChart = new PieChart
        {
            Entries = entries,
            LabelTextSize = 35,
            BackgroundColor = SKColors.Transparent
        };
    }

    private string GetRandomColorHex()
    {
        Random rand = new Random();
        return $"#{rand.Next(0x1000000):X6}";
    }

    internal void RefreshPage()
    {
        LoadPieChartFromCharacters(); // Recharge les données si besoin
    }
}
