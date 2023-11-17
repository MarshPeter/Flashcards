namespace Flashcards;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;

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
                stack_name VARCHAR(MAX) NOT NULL,
                date_created DATETIME NOT NULL
            );",
            @"IF OBJECT_ID(N'Flashcard', N'U') IS NULL
            CREATE TABLE Flashcard (
                flashcard_id INTEGER IDENTITY(1,1) PRIMARY KEY,
                stack_id INTEGER NOT NULL FOREIGN KEY REFERENCES Stack(stack_id),
                front_text VARCHAR(MAX) NOT NULL,
                back_text VARCHAR(MAX) NOT NULL
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

        MultiDatabaseRequest(tables);
    }

    public int NewStack(string name)
    {
        return NewStack(name, new List<Flashcard>(){});
    }

    public int NewStack(string name, List<Flashcard> flashcards)
    {
        // ensure stack is in database
        string checkStackQuery = @$"
        BEGIN 
            IF NOT EXISTS (SELECT * FROM STACK WHERE stack_name = '{name}')
            BEGIN
                INSERT INTO Stack (stack_name, date_created)
                VALUES ('{name}', GETDATE())
            END
        END;";

        SingleDatabaseRequest(checkStackQuery);

        string stackIdQuery = @$"
        SELECT * FROM Stack WHERE stack_name = '{name}';
        ";

        Stack owningStack = new Stack();

        try
        {
            using SqlConnection conn = new(ConnectionString);

            conn.Open();
            SqlCommand tableCmd = conn.CreateCommand();
            tableCmd.CommandText = stackIdQuery;

            SqlDataReader reader = tableCmd.ExecuteReader();

            while (reader.Read())
            {
                owningStack.StackId = reader.GetInt32(0);
            }

            conn.Close();
        } catch (Exception e)
        {
            Console.WriteLine("AN ERROR HAS OCCURRED");
            Console.WriteLine("Query: " + stackIdQuery);
            Console.WriteLine(e);
            Environment.Exit(1);
        }

        foreach (Flashcard flashcard in flashcards)
        {
            flashcard.StackId = owningStack.StackId;
            owningStack.Flashcards.Add(flashcard);
        }

        AddFlashcardsToStack(flashcards);
        return owningStack.StackId;
    }

    public void AddFlashcardsToStack(List<Flashcard> flashcards)
    {
        List<string> queries = new();

        foreach (Flashcard flashcard in flashcards)
        {
            queries.Add($@"
            INSERT INTO Flashcard (stack_id, front_text, back_text)
            VALUES ({flashcard.StackId}, '{flashcard.FrontText}', '{flashcard.BackText}')
            ");
        }

        MultiDatabaseRequest(queries);
    }

    public void UpdateFlashcard(Flashcard flashcard)
    {
        string query = $@"
        UPDATE Flashcard
        SET front_text = '{flashcard.FrontText}', back_text = '{flashcard.BackText}'
        WHERE flashcard_id = {flashcard.FlashcardId};
        ";

        SingleDatabaseRequest(query);
    }

    public void UpdateStackName(Stack stack)
    {
        string query = $@"
        UPDATE Stack
        SET stack_name = '{stack.StackName}'
        WHERE stack_id = {stack.StackId}
        ";

        SingleDatabaseRequest(query);
    }

    public void DeleteStack(Stack stack)
    {
        string studySessionStackDelete = $@"
        DELETE FROM StudySessionStack WHERE stack_id = {stack.StackId}
        ";

        string deleteFlashcardsFromStack = $@"
        DELETE FROM Flashcard WHERE stack_id = {stack.StackId}
        ";

        string deleteStack = $@"
        DELETE FROM Stack WHERE stack_id = {stack.StackId};
        ";

        SingleDatabaseRequest(studySessionStackDelete);
        SingleDatabaseRequest(deleteFlashcardsFromStack);
        SingleDatabaseRequest(deleteStack);
    }

    public void DeleteFlashcard(Flashcard flashcard)
    {
        string deleteQuery = $@"
        DELETE FROM Flashcard WHERE flashcard_id = {flashcard.FlashcardId};
        ";

        SingleDatabaseRequest(deleteQuery);
    }

    public void SaveSessionData(Session session)
    {
        string sessionQuery = $@"
        INSERT INTO StudySession (session_date) VALUES (GETDATE());
        ";
        SingleDatabaseRequest(sessionQuery);

        string retrieveSessionId = $@"
        SELECT * FROM StudySession WHERE study_session_id = SCOPE_IDENTITY()
        ";

        List<string> sessionStackQueries = new();

        try
        {
            using SqlConnection conn = new(ConnectionString);

            conn.Open();
            SqlCommand tableCmd = conn.CreateCommand();
            tableCmd.CommandText = retrieveSessionId;

            SqlDataReader reader = tableCmd.ExecuteReader();

            while (reader.Read())
            {
                session.SessionId = reader.GetInt32(0);
            }

            conn.Close();
        } catch (Exception e)
        {
            Console.WriteLine("AN ERROR HAS OCCURRED");
            Console.WriteLine("Query: " + retrieveSessionId);
            Console.WriteLine(e);
            Environment.Exit(1);
        }

        foreach (SessionStack stack in session.StacksForSession)
        {
            sessionStackQueries.Add($@"
            INSERT INTO StudySessionStack (study_session_id, stack_id, session_stack_score, max_stack_score)
            VALUES ({session.SessionId}, {stack.StackId}, {stack.Score}, {stack.MaxScore})
            ");
        }

        MultiDatabaseRequest(sessionStackQueries);
    }

    public List<Stack> GetAllStacks()
    {
        string stackQuery = "SELECT * FROM Stack";
        List<Stack> allStacks = new();

        try
        {
            using SqlConnection conn = new(ConnectionString);

            conn.Open();
            SqlCommand tableCmd = conn.CreateCommand();
            tableCmd.CommandText = stackQuery;

            SqlDataReader reader = tableCmd.ExecuteReader();
            int counter = 1;

            while (reader.Read())
            {
                Stack stack = new();

                stack.StackId = reader.GetInt32(0);
                stack.DisplayId = counter++;
                stack.StackName = reader.GetString(1);
                stack.DateCreated = reader.GetDateTime(2);

                allStacks.Add(stack);
            }

            conn.Close();
        } catch (Exception e)
        {
            Console.WriteLine("AN ERROR HAS OCCURRED");
            Console.WriteLine("Query: " + stackQuery);
            Console.WriteLine(e);
            Environment.Exit(1);
        }
    

        return allStacks;
    }

    public List<Flashcard> RetrieveFlashcards(Stack stack)
    {
        string flashcardQuery = $@"
        SELECT * FROM Flashcard WHERE stack_id = {stack.StackId}
        ";

        List<Flashcard> flashcardsForStack = new();

        try
        {
            using SqlConnection conn = new(ConnectionString);
            conn.Open();
            SqlCommand tableCmd = conn.CreateCommand();
            tableCmd.CommandText = flashcardQuery;

            SqlDataReader reader = tableCmd.ExecuteReader();

            int counter = 1;
            while (reader.Read())
            {
                Flashcard flashcard = new();
                flashcard.FlashcardId = reader.GetInt32(0);
                flashcard.StackId = reader.GetInt32(1);
                flashcard.FrontText = reader.GetString(2);
                flashcard.BackText = reader.GetString(3);
                flashcard.DisplayFlashcardId = counter++;
                flashcardsForStack.Add(flashcard);
            }
        } catch (Exception e)
        {
            Console.WriteLine("AN ERROR HAS OCCURRED");
            Console.WriteLine("Query: " + flashcardQuery);
            Console.WriteLine(e);
            Environment.Exit(1);
        }

        return flashcardsForStack;
    }

    private void SingleDatabaseRequest(string query)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand tableCmd = conn.CreateCommand();
                tableCmd.CommandText = query;
                tableCmd.ExecuteNonQuery();
                conn.Close();
            }
        } catch (Exception e)
        {
            Console.WriteLine("AN ERROR HAS OCCURRED");
            Console.WriteLine("Query: " + query);
            Console.WriteLine(e);
            Environment.Exit(1);
        }
    }

    private void MultiDatabaseRequest(List<string> queries)
    {
        try
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand tableCmd = conn.CreateCommand();
                foreach (string query in queries)
                {
                    tableCmd.CommandText = query;
                    tableCmd.ExecuteNonQuery();
                }
                conn.Close();
            }
        } catch (Exception e)
        {
            Console.WriteLine("AN ERROR HAS OCCURRED");
            Console.WriteLine(queries[0]);
            Console.WriteLine(e);
            Environment.Exit(1);
        }
    }

    private string ConnectionString {get; set;}
}