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
    public async Task<List<AnimeCharacter>> LoadData(List<AnimeCharacter> existingData)
    {
        var updatedList = new List<AnimeCharacter>();

        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Sélectionnez un fichier CSV"
        });

        if (result != null)
        {
            var lines = new List<string>();

            try
            {
                using var stream = new FileStream(result.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream, Encoding.UTF8);

                while (!reader.EndOfStream)
                {
                    var line = await reader.ReadLineAsync();
                    if (line != null)
                        lines.Add(line);
                }
            }
            catch (IOException)
            {
                await Shell.Current.DisplayAlert("Erreur",
                    "Impossible de lire le fichier. Il est peut-être utilisé par une autre application. Veuillez le fermer ou en faire une copie.",
                    "OK");
                return existingData;
            }

            if (lines.Count < 2)
                return existingData; // Aucune donnée à traiter

            var headers = lines[0].Split(';');
            var properties = typeof(AnimeCharacter).GetProperties();

            var headerList = headers.Select(h => h.Trim()).ToList();
            var modelProperties = properties.Select(p => p.Name).ToList();

            var invalidHeaders = headerList
                .Where(h => !modelProperties.Contains(h, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (invalidHeaders.Any())
            {
                await Shell.Current.DisplayAlert("Erreur CSV",
                    "Le fichier contient des colonnes non reconnues : " + string.Join(", ", invalidHeaders),
                    "OK");
                return existingData;
            }

            for (int i = 1; i < lines.Count; i++)
            {
                var values = lines[i].Split(';');

                if (values.All(v => string.IsNullOrWhiteSpace(v)))
                    continue;

                AnimeCharacter obj = new();

                for (int j = 0; j < headers.Length; j++)
                {
                    var property = properties.FirstOrDefault(p => p.Name.Equals(headers[j], StringComparison.OrdinalIgnoreCase));
                    if (property != null && j < values.Length)
                    {
                        try
                        {
                            string input = values[j].Trim();
                            if (string.IsNullOrWhiteSpace(input)) continue;

                            object? value = null;

                            if (property.PropertyType == typeof(int))
                            {
                                if (int.TryParse(input, out var intVal))
                                    value = intVal;
                                else
                                    throw new Exception($"Valeur entière invalide pour {property.Name} : {input}");
                            }
                            else if (property.PropertyType == typeof(double))
                            {
                                if (double.TryParse(input, out var doubleVal))
                                    value = doubleVal;
                                else
                                    throw new Exception($"Valeur décimale invalide pour {property.Name} : {input}");
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
                            System.Diagnostics.Debug.WriteLine($"⚠️ Erreur de conversion (ligne {i + 1}) : {ex.Message}");
                        }
                    }
                }

                // 🔐 Ajouter l'utilisateur courant à UserIds s'il n'est pas déjà là
                var currentUserId = Globals.CurrentUser.Id.ToString();

                if (obj.UserIds == null)
                    obj.UserIds = new List<string>();

                if (!obj.UserIds.Contains(currentUserId))
                    obj.UserIds.Add(currentUserId);

                if (string.IsNullOrWhiteSpace(obj.Id) || !obj.Id.All(char.IsDigit) ||
                    string.IsNullOrWhiteSpace(obj.Name) ||
                    string.IsNullOrWhiteSpace(obj.Description) ||
                    string.IsNullOrWhiteSpace(obj.Picture) ||
                    string.IsNullOrWhiteSpace(obj.Sound) ||
                    string.IsNullOrWhiteSpace(obj.SpecialAttack) ||
                    string.IsNullOrWhiteSpace(obj.Origin))
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Ligne ignorée (incomplète ou invalide) : ID = {obj.Id}");
                    continue;
                }

                var existing = existingData.FirstOrDefault(e => e.Id == obj.Id);
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

            var missing = existingData.Where(x => !updatedList.Any(y => y.Id == x.Id));
            updatedList.AddRange(missing);
        }

        return updatedList;
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
