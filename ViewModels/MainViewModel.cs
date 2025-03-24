using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using AnimeDex.Models;
using System.Diagnostics;

namespace AnimeDex.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<AnimeCharacter> Characters { get; set; } = new();

        private const string FileName = "characters.json";
        private string FilePath => Path.Combine(GetProjectDataPath(), FileName);



        public MainViewModel()
        {
            LoadCharacters();
        }

        public void AddCharacter(AnimeCharacter character)
        {
            Characters.Add(character);
            SaveCharacters();
        }

        public void SaveCharacters()
        {
            var json = JsonSerializer.Serialize(Characters);
            File.WriteAllText(FilePath, json);
        }

        public void LoadCharacters()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var characters = JsonSerializer.Deserialize<ObservableCollection<AnimeCharacter>>(json);
                if (characters != null)
                    Characters = characters;
            }
        }

        private string GetProjectDataPath()
        {
            var projectRoot = AppContext.BaseDirectory;

            // Remonte jusqu'au dossier racine du projet "AnimeDex"
            var path = Path.GetFullPath(Path.Combine(projectRoot, @"..\..\..\..\Data"));

            // Crée le dossier s'il n'existe pas
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
