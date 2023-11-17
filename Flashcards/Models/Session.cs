namespace Flashcards;

public class Session
{
    public Session()
    {
        // initialise list
        StacksForSession = new();
    }
    public int SessionId {get; set;}
    public DateTime Date {get; set;}
    public List<SessionStack> StacksForSession {get; set;} 
}