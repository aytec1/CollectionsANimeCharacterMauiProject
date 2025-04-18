using System;
using MongoDB.Bson;
using MongoDB.Driver;

class Program
{
    static void TestMongoConnection()
    {
        string connectionString = "mongodb://student:IAmTh3B3st@185.157.245.38:5003";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase("Anime");
        var collection = database.GetCollection<BsonDocument>("AnimeCharacter");

        Console.WriteLine("Connexion réussie à MongoDB!");

        var documents = collection.Find(new BsonDocument()).ToList();
        foreach (var document in documents)
        {
            Console.WriteLine(document.ToString());
        }
    }
}
