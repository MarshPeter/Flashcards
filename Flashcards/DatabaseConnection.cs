namespace Flashcards;
using System.Configuration;
using System.Data.SqlClient;

public class DatabaseConnection
{
    public DatabaseConnection(string connectionString)
    {
        ConnectionString = connectionString;
        InitialiseTables();
    }

    private void InitialiseTables()
    {
        List<string> tables = new(){
            @"IF OBJECT_ID(N'Stack', N'U') IS NULL
            CREATE TABLE Stack (
                stack_id INTEGER IDENTITY(1,1) PRIMARY KEY,
                stack_name TEXT NOT NULL,
                date_created DATETIME NOT NULL
            );",
            @"IF OBJECT_ID(N'Flashcard', N'U') IS NULL
            CREATE TABLE Flashcard (
                flashcard_id INTEGER IDENTITY(1,1) PRIMARY KEY,
                stack_id INTEGER NOT NULL FOREIGN KEY REFERENCES Stack(stack_id),
                front_text TEXT NOT NULL,
                back_text TEXT NOT NULL
            );",
            @"IF OBJECT_ID(N'StudySession', N'U') IS NULL
            CREATE TABLE StudySession (
                study_session_id INTEGER IDENTITY(1,1) PRIMARY KEY,
                session_date DATETIME NOT NULL
            );",
            @"IF OBJECT_ID(N'StudySessionStack', N'U') IS NULL
            CREATE TABLE StudySessionStack (
                study_session_stack_id INTEGER IDENTITY(1,1) PRIMARY KEY,
                study_session_id INTEGER FOREIGN KEY REFERENCES StudySession(study_session_id),
                stack_id INTEGER FOREIGN KEY REFERENCES Stack(stack_id),
                session_stack_score INTEGER NOT NULL,
                max_stack_score INTEGER NOT NULL
            );"
        };

        try
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand tableCmd = conn.CreateCommand();
                foreach (string query in tables)
                {
                    tableCmd.CommandText = query;
                    tableCmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        } catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }

    private string ConnectionString {get; set;}
}