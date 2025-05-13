using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MyApp.Model;

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
            _users.InsertOne(user);
            Console.WriteLine("Utilisateur ajouté !");
        }

        public List<User> GetAllUsers()
        {
            return _users.Find(u => true).ToList();
        }

        // ✅ Méthode async attendue par UserListViewModel
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _users.Find(u => true).ToListAsync();
        }
    }
}
