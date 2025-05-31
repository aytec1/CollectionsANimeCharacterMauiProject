using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MyApp.Model;
using Microsoft.Maui.Networking;

namespace MyApp.Service
{
    public class MongoUserService
    {

        private readonly IMongoCollection<User> _users;

        public MongoUserService()
        {
            
            string connectionString = "mongodb://student:IAmTh3B3st@185.157.245.38:5003";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("DBAnime");
            _users = database.GetCollection<User>("Users");
        }

        public void AddUser(User user)
        {
            if (!IsConnected())
            {
                Console.WriteLine("❌ Pas de connexion Internet pour accéder à MongoDB.");
                return; // ou return une liste vide, ou afficher une alerte selon le contexte
            }
            user.Password = HashPassword(user.Password);
            _users.InsertOne(user);
            Console.WriteLine("Utilisateur ajouté avec mot de passe sécurisé (PBKDF2) !");
        }

        public List<User> GetAllUsers()
        {
            if (!IsConnected())
            {
                Console.WriteLine("❌ Pas de connexion Internet pour accéder à MongoDB.");
                return null; // ou return une liste vide, ou afficher une alerte selon le contexte
            }
            return _users.Find(u => true).ToList();
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            if (!IsConnected())
            {
                await Shell.Current.DisplayAlert("Erreur réseau", "Pas de connexion Internet. Données non sauvegardées.", "OK");
                return null;
            }
            return await _users.Find(u => true).ToListAsync();
        }

        public bool AuthenticateUser(string email, string password)
        {
            if (!IsConnected())
            {
                // Appeler DisplayAlert sur le thread UI
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await Shell.Current.DisplayAlert(
                        "Erreur réseau",
                        "Vous devez être connecté à Internet pour vous authentifier.",
                        "OK");
                });

                return false;
            }
            var user = _users.Find(u => u.Email == email).FirstOrDefault();
            if (user == null) return false;

            return VerifyPassword(password, user.Password);
        }

        private string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            var hash = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hashBytes = hash.GetBytes(32);

            byte[] hashWithSalt = new byte[48];
            Array.Copy(salt, 0, hashWithSalt, 0, 16);
            Array.Copy(hashBytes, 0, hashWithSalt, 16, 32);

            return Convert.ToBase64String(hashWithSalt);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            byte[] hashWithSalt = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[16];
            Array.Copy(hashWithSalt, 0, salt, 0, 16);

            var hash = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            byte[] hashBytes = hash.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hashWithSalt[i + 16] != hashBytes[i])
                    return false;
            }

            return true;
        }
        private bool IsConnected()
        {
            var access = Connectivity.Current.NetworkAccess;
            return access == NetworkAccess.Internet;
        }
    }
}
