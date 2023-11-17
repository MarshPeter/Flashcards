namespace Flashcards;

using System.ComponentModel.DataAnnotations;
using ConsoleTableExt;

public class TableDisplay
{
    public void DisplayBothSidesOfFlashcard(Flashcard flashcard)
    {
        List<List<object>> tableData = new(){ new List<object>(){flashcard.FrontText!, flashcard.BackText!}};
        ConsoleTableBuilder
            .From(tableData)
            .WithTitle("Flashcard")
            .WithColumn("Front Text", "Back Text")
            .ExportAndWriteLine();
    }

    public void DisplayAllFlashcards(Stack stack)
    {
        List<List<object>> tableData = new();

        foreach (Flashcard flashcard in stack.Flashcards)
        {
            tableData.Add(new List<object>(){
                flashcard.DisplayFlashcardId,
                flashcard.FrontText!,
                flashcard.BackText!
            });
        }

        ConsoleTableBuilder
            .From(tableData)
            .WithTitle("All Flashcards")
            .WithColumn("Id", "FrontText", "BackText")
            .ExportAndWriteLine();
    }

}