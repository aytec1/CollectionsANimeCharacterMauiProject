using Microcharts;
using Microsoft.Maui.Graphics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using MyApp.Model;
using MyApp.Service;
using System.Threading.Tasks;

namespace MyApp.ViewModel;

public partial class GraphViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string? CharacterOrigin { get; set; }

    [ObservableProperty]
    public partial Chart MyObservableChart { get; set; } = new PieChart();

    private readonly HashSet<string> usedColors = new();
    private readonly Random rand = new();

    public GraphViewModel()
    {
        LoadPieChartFromCharacters();
    }

    private void LoadPieChartFromCharacters()
    {
        usedColors.Clear();

        var uniqueCharacters = Globals.MyAnimeCharacters
            .Where(c => !string.IsNullOrEmpty(c.Origin) && !string.IsNullOrWhiteSpace(c.Id))
            .GroupBy(c => c.Id)                  // Éliminer les doublons par Id
            .Select(g => g.First())              // Garder un seul exemplaire
            .ToList();

        var groupedByOrigin = uniqueCharacters
            .GroupBy(c => c.Origin)
            .Select(g => new { Origin = g.Key, Count = g.Count() })
            .ToList();

        var entries = groupedByOrigin.Select(g => new ChartEntry(g.Count)
        {
            Label = g.Origin,
            ValueLabel = $"{g.Count} personnage(s)",
            Color = SKColor.Parse(GetUniqueRandomColorHex()),
            ValueLabelColor = SKColors.White,
            TextColor = SKColors.White
        }).ToArray();

        MyObservableChart = new RadarChart
        {
            Entries = entries,
            LabelTextSize = 35,
            BackgroundColor = SKColors.Transparent,
            MinValue = 0
        };
    }


    private string GetUniqueRandomColorHex()
    {
        string color;
        do
        {
            color = $"#{rand.Next(0x1000000):X6}";
        }
        while (!usedColors.Add(color)); // Ajoute au HashSet, retourne false si déjà présent

        return color;
    }

    internal void RefreshPage()
    {
        LoadPieChartFromCharacters();
    }


}
