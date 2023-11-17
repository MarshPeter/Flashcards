namespace Flashcards;

public class Flashcard
{
    public int FlashcardId {get; set;}
    public int DisplayFlashcardId {get; set;}
    public int StackId {get; set;}
    public string? FrontText {get; set;}
    public string? BackText {get; set;}
}