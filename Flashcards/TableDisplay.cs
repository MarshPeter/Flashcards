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

    public void ListStacks(List<Stack> stacks)
    {
        List<List<object>> tableData = new();

        foreach (Stack stack in stacks)
        {
            tableData.Add(new List<object>(){
                stack.DisplayId,
                stack.StackName!
            });
        }

        ConsoleTableBuilder
            .From(tableData)
            .WithTitle("Stacks")
            .WithColumn("Id", "Name")
            .ExportAndWriteLine();
    }

}