using Flashcards;

using System.Configuration;

string connectionString = ConfigurationManager.AppSettings.Get("connectionString")!; 

DatabaseConnection db = new DatabaseConnection(connectionString);
TableDisplay tb = new TableDisplay();
ConsoleInterface userInteraction = new ConsoleInterface(db, tb);

userInteraction.StartingMenu();
