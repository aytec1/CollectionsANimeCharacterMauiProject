using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Collections.Generic;


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
}