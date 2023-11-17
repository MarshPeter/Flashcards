namespace Flashcards;

public class Stack
{

    public Stack()
    {
        Flashcards = new();
    }

    public int StackId {get; set;}

    public int DisplayId {get; set;}

    public string? StackName {get; set;}

    public DateTime DateCreated {get; set;}

    public List<Flashcard> Flashcards {get; set;}
}