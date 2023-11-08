using Flashcards;
using System.Configuration;

Console.WriteLine("Hello, World!");

string connectionString = ConfigurationManager.AppSettings.Get("connectionString")!; 

DatabaseConnection db = new DatabaseConnection(connectionString);
