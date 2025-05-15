using CommunityToolkit.Mvvm.ComponentModel;
using MyApp.Model;
using Plugin.Maui.Audio;

namespace MyApp.ViewModel;

[QueryProperty(nameof(Id), "selectedCharacter")]
public partial class CharacterPreviewViewModel : ObservableObject
{
    private readonly IAudioManager _audioManager = AudioManager.Current;
    private IAudioPlayer? _player; // 🔊 Ajout : garder la référence

    [ObservableProperty] private string id;
    [ObservableProperty] private string picture;
    [ObservableProperty] private string specialAttack;

    public async Task InitializeAsync()
    {
        var character = Globals.MyAnimeCharacters.FirstOrDefault(c => c.Id == Id);
        if (character is null)
        {
            System.Diagnostics.Debug.WriteLine("❌ Aucun personnage trouvé avec cet ID !");
            return;
        }

        Picture = character.Picture;
        SpecialAttack = character.SpecialAttack;

        try
        {
            var stream = await FileSystem.OpenAppPackageFileAsync(character.Sound);
            _player = _audioManager.CreatePlayer(stream); // 🎵 Référence stockée
            _player.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Erreur audio : {ex.Message}");
        }
    }

    public void StopSound()
    {
        _player?.Stop();
        _player?.Dispose();
        _player = null;
    }
}
