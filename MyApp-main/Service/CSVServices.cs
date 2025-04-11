using CommunityToolkit.Maui.Storage;
using System;
using System.Collections.Generic;
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
            var lines = await File.ReadAllLinesAsync(result.FullPath, Encoding.UTF8);

            if (lines.Length < 2)
                return existingData; // Aucune donnée à traiter

            var headers = lines[0].Split(';');
            var properties = typeof(AnimeCharacter).GetProperties();

            var headerList = headers.Select(h => h.Trim()).ToList();
            var modelProperties = typeof(AnimeCharacter).GetProperties().Select(p => p.Name).ToList();

            var invalidHeaders = headerList.Where(h => !modelProperties.Contains(h, StringComparer.OrdinalIgnoreCase)).ToList();

            if (invalidHeaders.Any())
            {
                await Shell.Current.DisplayAlert("Erreur CSV",
                    "Le fichier contient des colonnes non reconnues : " + string.Join(", ", invalidHeaders),
                    "OK");
                return existingData;
            }


            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(';');

                // Ignore la ligne si elle est totalement vide ou remplie d'espaces
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
                            object value = Convert.ChangeType(values[j], property.PropertyType);
                            property.SetValue(obj, value);
                        }
                        catch
                        {
                            // Ignore les erreurs de conversion si la valeur est incorrecte
                        }
                    }
                }

                // Vérifie s'il existe déjà dans la collection (par Id)
                var existing = existingData.FirstOrDefault(e => e.Id == obj.Id);
                if (existing != null)
                {
                    // Mise à jour des propriétés
                    foreach (var prop in properties)
                    {
                        var newValue = prop.GetValue(obj);
                        prop.SetValue(existing, newValue);
                    }
                    updatedList.Add(existing);
                }
                else
                {
                    updatedList.Add(obj);
                }
            }

            // Ajoute les personnages qui ne sont pas dans le CSV mais qui existent déjà
            var missing = existingData.Where(x => !updatedList.Any(y => y.Id == x.Id));
            updatedList.AddRange(missing);
        }

        return updatedList;
    }

    public async Task PrintData<T>(List<T> data)
    {
        var csv = new StringBuilder();
        var properties = typeof(T).GetProperties();
        csv.AppendLine(string.Join(";", properties.Select(p => p.Name)));

        foreach (var item in data)
        {
            var values = properties.Select(p => p.GetValue(item)?.ToString() ?? "");
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
