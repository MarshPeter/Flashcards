namespace Flashcards;

public class ConsoleInterface
{
    public ConsoleInterface(DatabaseConnection db, TableDisplay tb)
    {
        Console.Clear();
        Console.WriteLine("Setting up database");
        DB = db;
        TbDisplay = tb;
        Console.WriteLine("Retrieving all stacks");
        Stacks = DB.GetAllStacks();
        Console.WriteLine("Setting up session");
        ThisSession = new();
        Console.WriteLine("Everything is ready, press any key to continue");
        Console.ReadKey();
    }

    public void StartingMenu()
    {
        Console.WriteLine("Welcome to the Flashcard Software");
        Console.WriteLine("--------------------------------------------------------------------");
        Console.WriteLine("Select any of the options below by choosing one of the corresponding numbers");
        Console.WriteLine("--------------------------------------------------------------------");
        Console.WriteLine("0 - Exit the program");
        Console.WriteLine("1 - Start a practice session");
        Console.WriteLine("2 - Create a new stack of cards");
        Console.WriteLine("3 - Look at an existing stack of cards");
        Console.WriteLine("--------------------------------------------------------------------");

        string? userChoice = Console.ReadLine();

        Console.Clear();
        if (userChoice == null)
        {
            InvalidChoice();
        }

        switch(userChoice?.Trim())
        {
            case "0":
                EndProgram();
                return;
            case "2":
                CreateStack();
                break;
            case "3":
                ViewStacks();
                break;
            default:
                InvalidChoice();
                break;
        }

    }

    // Case 0 choice section
    private void EndProgram()
    {
        Console.WriteLine("Thankyou for using the program");
        Console.WriteLine("Press any key to end the program");
        Console.ReadKey();
    } 

    // case 2 choice section
    private void CreateStack()
    {
        Console.WriteLine("Welcome to the Create a stack of cards section!");
        Console.WriteLine("--------------------------------------------------------------------");
        while (true)
        {
            Console.WriteLine("Enter a topic for your stack of cards, this will be the stack of card's name");
            Console.WriteLine("Alternatively, you can type r or R to return to the main menu");
            Console.Write("Enter the stack name: ");
            string? usersDesiredName = Console.ReadLine()?.Trim()?.ToLower();

            if (usersDesiredName == null)
            {
                InvalidChoice();
                Console.Clear();
                continue;
            }

            bool alreadyExists = false;

            foreach (Stack stack in Stacks)
            {
                if (usersDesiredName == stack.StackName)
                {
                    alreadyExists = true;
                    Console.WriteLine("That stack already exists and cannot be created again, would you like to add flashcards to that one instead?");
                    Console.WriteLine("'y' - for yes, anything else for no");
                    Console.WriteLine("--------------------------------------------------------------------");
                    Console.Write("Enter your choice > ");
                    string? choice = Console.ReadLine()?.Trim().ToLower();
                    if (choice == "y")
                    {
                        AddToStack(stack, true);
                        return;
                    } else
                    {
                        break;
                    }
                }
            }

            if (alreadyExists)
            {
                Console.WriteLine();
                Console.WriteLine("Since you have selected to not add to the already existing stack, you will need to try a differnet name");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
                Console.Clear();
                continue;
            }

            if (usersDesiredName == "r")
            {
                Console.WriteLine("OK you will be returned to the menu");
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();
            }

            //TODO ADD a validator here later
            bool validName = true;

            if (validName)
            {
                Stack newStack = new();
                newStack.StackName = usersDesiredName;
                Console.WriteLine("Your stack name is valid, therefore you can now add to it.");
                Console.WriteLine("It is very important to note however, if you end the stack creation before you finish adding a flashcard to it, the stack will not be saved in the database");
                Console.WriteLine("With all the said, press any key to continue");
                Console.ReadKey();
                AddToStack(newStack);
                return;
            }         
        }
    }

    private void AddToStack(Stack stack, bool alreadyExists = false)
    {
        bool stillAddingToStack = true;

        while (stillAddingToStack)
        {
            Console.Clear();
            Console.WriteLine($"This is a new flashcard to add to the stack titied {stack.StackName}");
            Console.WriteLine("--------------------------------------------------------------------");
            string? frontText = RetrieveText("Enter text you wish for the frontside of the flashcard");
            Console.WriteLine("--------------------------------------------------------------------");
            string? backText = RetrieveText("Enter text you wish for the backside of the flashcard");

            //TODO Add a validation method for card text
            bool validFrontText = true;
            bool validBackText = true;

            if (validFrontText && validBackText)
            {
                Flashcard flashcard = new();
                flashcard.FrontText = frontText;
                flashcard.BackText = backText; 
                TbDisplay.DisplayBothSidesOfFlashcard(flashcard);                
                bool response = ConfirmationMessage("Is this flashcard ok for you?");

                if (response)
                {
                    if (stack.Flashcards.Count == 0)
                    {
                        flashcard.DisplayFlashcardId = 1;
                    }
                    else
                    {
                        flashcard.DisplayFlashcardId = stack.Flashcards.Count + 1;
                    }
                    stack.Flashcards.Add(flashcard);
                    Console.WriteLine("Your flashcard has been added to the stack!");
                }
                else
                {
                    //TODO add a feature to allow the user to edit OR discard the flashcard
                    Console.WriteLine("Your flashcard has been discarded");
                }

                stillAddingToStack = ConfirmationMessage("Do you wish to add more cards to the stack?");
            }
        }

        Console.Clear();

        if (stack.Flashcards.Count == 0)
        {
            Console.WriteLine("It looks like your stack is empty.");
            bool response = ConfirmationMessage("The stack will not be uploaded with nothing in it, are you sure you wish to exit?");

            if (response)
            {
                Console.WriteLine("Understood, press any key to return to the main menu!");
                Console.ReadKey();
                return;
            } else
            {
                AddToStack(stack, alreadyExists);
                return;
            }
        }
        Console.WriteLine("This is the stack that will be entered");
        TbDisplay.DisplayAllFlashcards(stack);
        bool submitResponse = ConfirmationMessage("Are you ok with this being added to the database");

        if (submitResponse && alreadyExists)
        {
            int uploadedStackId = DB.NewStack(stack.StackName!, stack.Flashcards);           
            stack.StackId = uploadedStackId;
            stack.Flashcards = DB.RetrieveFlashcards(stack);

            for (int i = 0; i < this.Stacks.Count; i++)
            {
                if (stack.StackId == this.Stacks[i].StackId)
                {
                    this.Stacks[i] = stack;
                }
            }
        }
        else if (submitResponse)
        {
            int uploadedStackId = DB.NewStack(stack.StackName!, stack.Flashcards);           
            stack.StackId = uploadedStackId;
            stack.Flashcards = DB.RetrieveFlashcards(stack);
            
            this.Stacks.Add(stack);
        }
        else
        {
            // TODO: Probably allow for other alternatives.
            Console.WriteLine("Understood, the stack will be discarded!");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }

        Console.Clear();
        Console.WriteLine("Everything has been successfuly completed! ");
        Console.WriteLine("Press any key to return the main menu!");
        Console.ReadKey();
    }

    // option 3 functions
    public void ViewStacks()
    {
        Console.Clear();
        Console.WriteLine("Here are the stacks you can view");
        TbDisplay.ListStacks(Stacks);

    }

    // confirmation functions
    public bool ConfirmationMessage(string prompt)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            Console.WriteLine("Type  'y' for yes, or 'n' for no");
            string? response = Console.ReadLine();

            if (response == null)
            {
                Console.WriteLine("That was an invalid repsonse");
                continue;
            }

            switch (response.Trim().ToLower())
            {
                case "y":
                    return true;
                case "n":
                    return false;
                default:
                    Console.WriteLine("That was an invalid response");
                    break;
            }
        }
    }

    public string? RetrieveText(string prompt)
    {
        Console.WriteLine(prompt);
        Console.Write("Enter here > ");
        return Console.ReadLine()?.Trim();
    }

    // invalid choice code
    private void InvalidChoice()
    {
        Console.WriteLine("That was an invalid choice, Press any key to try again");
        Console.ReadKey();
    }

    public DatabaseConnection DB {get; set;}
    public List<Stack> Stacks {get; set;}
    public Session ThisSession {get; set;}
    public TableDisplay TbDisplay {get; set;}
}