using CommunityToolkit.Maui.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApp.Service;

public class CSVServices
{
    private readonly List<string> validOrigins = new()
    {
        "One Piece",
        "Naruto",
        "Bleach",
        "Dragon Ball",
        "My Hero Academia",
        "Hunter x Hunter"
    };

    public async Task<List<AnimeCharacter>> LoadData(List<AnimeCharacter> existingData)
    {
        System.Diagnostics.Debug.WriteLine("📥 [LoadData] méthode appelée");

        var lines = await ReadCsvLines();
        if (lines == null || lines.Count < 2)
        {
            System.Diagnostics.Debug.WriteLine("📄 Aucune ligne valide dans le fichier");
            return existingData;
        }

        System.Diagnostics.Debug.WriteLine($"📄 {lines.Count} lignes lues depuis le CSV");

        var headers = ParseHeaders(lines[0]);
        if (headers == null) return existingData;

        System.Diagnostics.Debug.WriteLine($"🧾 En-têtes CSV : {string.Join(", ", headers)}");

        var updatedList = new List<AnimeCharacter>();
        var currentUserId = Globals.CurrentUser.Id.ToString();
        int lignesIgnorees = 0;

        for (int i = 1; i < lines.Count; i++)
        {
            System.Diagnostics.Debug.WriteLine($"🔍 Traitement ligne {i + 1} : {lines[i]}");

            var obj = ParseCharacterLine(headers, lines[i]);
            if (obj == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Ligne ignorée : échec du parsing");
                lignesIgnorees++;
                continue;
            }

            AddCurrentUserIfMissing(obj, currentUserId);

            if (!ValidateCharacter(obj))
            {
                System.Diagnostics.Debug.WriteLine($"❌ Validation échouée : ID = {obj.Id}");
                lignesIgnorees++;
                continue;
            }

            MergeCharacterIntoList(obj, existingData, updatedList);
            System.Diagnostics.Debug.WriteLine($"✅ Ajouté ou mis à jour : {obj.Id} - {obj.Name}");
        }

        var missing = existingData.Where(x => !updatedList.Any(y => y.Id == x.Id));
        updatedList.AddRange(missing);

        if (lignesIgnorees > 0 && updatedList.Any())
        {
            await Shell.Current.DisplayAlert("Import CSV",
                $"{lignesIgnorees} ligne(s) ont été ignorées car incomplètes ou invalides.",
                "OK");
        }

        return updatedList;
    }

    private async Task<List<string>?> ReadCsvLines()
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Sélectionnez un fichier CSV"
        });

        if (result == null) return null;

        var lines = new List<string>();

        try
        {
            using var stream = new FileStream(result.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null) lines.Add(line);
            }
        }
        catch (IOException)
        {
            await Shell.Current.DisplayAlert("Erreur",
                "Impossible de lire le fichier. Il est peut-être utilisé par une autre application.", "OK");
            return null;
        }

        return lines;
    }

    private List<string>? ParseHeaders(string headerLine)
    {
        var headers = headerLine.Split(';').Select(h => h.Trim()).ToList();
        var modelProperties = typeof(AnimeCharacter).GetProperties().Select(p => p.Name).ToList();

        var invalidHeaders = headers.Where(h => !modelProperties.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();

        if (invalidHeaders.Any())
        {
            Shell.Current.DisplayAlert("Erreur CSV",
                "Le fichier contient des colonnes non reconnues : " + string.Join(", ", invalidHeaders),
                "OK").Wait();
            return null;
        }

        return headers;
    }

    private AnimeCharacter? ParseCharacterLine(List<string> headers, string line)
    {
        var values = line.Split(';');
        if (values.All(v => string.IsNullOrWhiteSpace(v))) return null;

        var obj = new AnimeCharacter();
        var properties = typeof(AnimeCharacter).GetProperties();

        for (int j = 0; j < headers.Count; j++)
        {
            var property = properties.FirstOrDefault(p => p.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase));
            if (property == null || j >= values.Length) continue;

            try
            {
                string input = values[j].Trim();
                if (string.IsNullOrWhiteSpace(input)) continue;

                object? value = null;

                if (property.PropertyType == typeof(int))
                {
                    if (int.TryParse(input, out var intVal)) value = intVal;
                }
                else if (property.PropertyType == typeof(double))
                {
                    if (double.TryParse(input, out var doubleVal)) value = doubleVal;
                }
                else if (property.PropertyType == typeof(string))
                {
                    value = input;
                }

                if (value != null)
                    property.SetValue(obj, value);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Erreur de conversion : {ex.Message}");
            }
        }

        return obj;
    }

    private void AddCurrentUserIfMissing(AnimeCharacter obj, string userId)
    {
        if (obj.UserIds == null)
            obj.UserIds = new List<string>();

        if (!obj.UserIds.Contains(userId))
            obj.UserIds.Add(userId);
    }

    private bool ValidateCharacter(AnimeCharacter obj)
    {
        if (string.IsNullOrWhiteSpace(obj.Id) || !obj.Id.All(char.IsDigit) ||
            string.IsNullOrWhiteSpace(obj.Name) ||
            string.IsNullOrWhiteSpace(obj.Description) ||
            string.IsNullOrWhiteSpace(obj.Picture) ||
            string.IsNullOrWhiteSpace(obj.Sound) ||
            string.IsNullOrWhiteSpace(obj.SpecialAttack) ||
            string.IsNullOrWhiteSpace(obj.Origin))
        {
            return false;
        }

        if (!validOrigins.Contains(obj.Origin))
        {
            System.Diagnostics.Debug.WriteLine($"❌ Origine non reconnue : {obj.Origin}");
            return false;
        }

        return true;
    }

    private void MergeCharacterIntoList(AnimeCharacter obj, List<AnimeCharacter> existingData, List<AnimeCharacter> updatedList)
    {
        var existing = existingData.FirstOrDefault(e => e.Id == obj.Id);
        var properties = typeof(AnimeCharacter).GetProperties();

        if (existing != null)
        {
            foreach (var prop in properties)
            {
                var newValue = prop.GetValue(obj);
                if (newValue != null && prop.Name != "UserIds")
                    prop.SetValue(existing, newValue);
            }

            foreach (var uid in obj.UserIds)
            {
                if (!existing.UserIds.Contains(uid))
                    existing.UserIds.Add(uid);
            }

            updatedList.Add(existing);
        }
        else
        {
            updatedList.Add(obj);
        }
    }

    public async Task PrintData<T>(List<T> data)
    {
        var csv = new StringBuilder();
        var properties = typeof(T).GetProperties();

        var filteredData = data
            .Where(item =>
            {
                var userIdsProp = typeof(T).GetProperty("UserIds");
                if (userIdsProp != null)
                {
                    var value = userIdsProp.GetValue(item);
                    if (value is List<string> userIds && userIds.Any())
                        return true;
                }
                return false;
            })
            .ToList();

        csv.AppendLine(string.Join(";", properties.Select(p => p.Name)));

        foreach (var item in filteredData)
        {
            var values = properties.Select(p =>
            {
                var val = p.GetValue(item);
                if (val is List<string> list)
                    return string.Join(",", list);
                return val?.ToString() ?? "";
            });
            csv.AppendLine(string.Join(";", values));
        }

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv.ToString()));
        var fileSaverResult = await FileSaver.Default.SaveAsync("Collection.csv", stream);

        if (!fileSaverResult.IsSuccessful)
        {
            // Gérer les erreurs si besoin
        }
    }
}