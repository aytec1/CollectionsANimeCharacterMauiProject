using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Collections.Generic;

public class User
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("firstname")]
    public string FirstName { get; set; }

    [BsonElement("lastname")]
    public string LastName { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }

    [BsonElement("role")]
    public string Role { get; set; }
}
