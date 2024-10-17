using System.Text.Json;

(new IndividualProject1.App()).Run();

namespace IndividualProject1
{
    public class App
    {
        private List<Task> ToDoList = new List<Task>();

        static string FILENAME = $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}ToDoList.json";

        public void Run()
        {
            if (File.Exists(FILENAME))
                LoadToDoList();

            if (ToDoList.Count < 1)
                GenerateSampleTasks();

            while (true)
            {
            GoToMainMenu:
                printMainMenu();

                string? action = GetInput();

                if (action is null)
                    continue;

                switch (action)
                {
                    case "1": // Show all tasks and enable sorting by project or date
                        if (ToDoList.Count < 1)
                        {
                            PrintTextWithColor("\nThere is no task to show.", ConsoleColor.Red);
                            break;
                        }

                        printToDoList(ToDoList);

                        while (true)
                        {
                            string m = Text.GO_BACK + "\n" + Text.SORT_OPTIONS;
                            PrintTextWithColor(m, ConsoleColor.Blue);

                            string? input = GetInput();

                            if (input is null)
                                continue;

                            if (IsTokenEqual(input, "q"))
                                break;

                            if (IsTokenEqual(input, "1"))
                                printToDoList(SortTaskByDate(ToDoList));
                            else if (IsTokenEqual(input, "2"))
                                printToDoList(SortTaskByProject(ToDoList));
                        }

                        break;
                    case "2": // Add a new task
                        while (true)
                        {
                            PrintTextWithColor(Text.GO_BACK, ConsoleColor.Blue);
                            Console.Write("Enter a title to add a new task: ");

                            string? title = GetInput();

                            if (title is null)
                                continue;

                            if (IsTokenEqual(title, "q"))
                                break;

                            Task? aTask = FindTaskByTitle(title);
                            if (aTask is not null)
                            {
                                PrintTextWithColor("You have already a task with that name.", ConsoleColor.Red);
                                printTask(aTask);
                                continue;
                            }

                            int? project;
                            while (true) // Gets project type from user
                            {
                                PrintTextWithColor(Text.PROJECT_OPTIONS, ConsoleColor.Blue);
                                string? input = GetInput();

                                project = GetInputAsInteger(input!);

                                if (project is null)
                                    continue;

                                break;
                            }

                            DateTime date;
                            while (true) // Gets date from user
                            {
                                PrintTextWithColor("Enter a date (yyyy-MM-dd): ", ConsoleColor.Blue);
                                string? _date = GetInput();

                                if (!DateTime.TryParse(_date, out DateTime date2))
                                    continue;

                                date = Convert.ToDateTime(date2);

                                break;
                            }

                            ToDoList.Add(new Task { Title = title, DueDate = date, Project = (Project)(project - 1), Status = false });

                            PrintTextWithColor("Press 'Enter' key to continue adding new tasks or enter 'q' for main menu", ConsoleColor.Blue);

                            string? continueOrQuit = GetInput();

                            if (continueOrQuit is null)
                                continue;

                            if (IsTokenEqual(continueOrQuit!, "q"))
                            {
                                printToDoList(ToDoList);
                                goto GoToMainMenu;
                            }
                        }

                        break;
                    case "3": // Edit task
                        PrintTextWithColor(Text.GO_BACK, ConsoleColor.Blue);

                        if (ToDoList.Count < 1)
                        {
                            PrintTextWithColor("\nThere is no task to edit.", ConsoleColor.Red);
                            break;
                        }

                        while (true) // Gets title of a task from user to prepare that task for editing
                        {
                            Console.Write("Enter your task's title to edit it: ");

                            string? title = GetInput();

                            if (title is null)
                                continue;

                            if (IsTokenEqual(title, "q"))
                                break;

                            Task? taskTobeUpdated = FindTaskByTitle(title);
                            if (taskTobeUpdated is null)
                            {
                                PrintTextWithColor($"\nThere is no task with that name.", ConsoleColor.Red);
                                continue;
                            }

                            while (true)
                            {
                                PrintTextWithColor(Text.UPDATE_OPTIONS, ConsoleColor.DarkYellow);

                                string? input = GetInput();

                                if (input is null)
                                    continue;

                                if (IsTokenEqual(input, "1")) // Edit title of the task
                                {
                                    Console.Write("New title: ");
                                    string? newTitle = GetInput();

                                    if (newTitle is null)
                                        continue;

                                    Task? aNewTitle = FindTaskByTitle(newTitle);
                                    if (aNewTitle is not null)
                                    {
                                        PrintTextWithColor("You have already a task with that name.", ConsoleColor.Red);
                                        printTask(aNewTitle);
                                        continue;
                                    }

                                    taskTobeUpdated.Title = newTitle;
                                    PrintTextWithColor("You have successfully updated your task's title.", ConsoleColor.Green);
                                }
                                else if (IsTokenEqual(input, "2")) // Mark the task as done
                                {
                                    taskTobeUpdated.Status = true;
                                    PrintTextWithColor(Text.UPDATE_STATUS, ConsoleColor.Green);
                                }
                                else if (IsTokenEqual(input, "3")) // Mark the task as not done
                                {
                                    taskTobeUpdated.Status = false;
                                    PrintTextWithColor(Text.UPDATE_STATUS, ConsoleColor.Green);
                                }
                                else if (IsTokenEqual(input, "4")) // Delete the task
                                {
                                    ToDoList.RemoveAt(ToDoList.IndexOf(taskTobeUpdated));
                                    PrintTextWithColor("\nYou have successfully deleted your task.\n", ConsoleColor.Green);
                                    printToDoList(ToDoList);

                                    goto GoToMainMenu;
                                }
                                else if (IsTokenEqual(input, "5")) // Go to main menu
                                    goto GoToMainMenu;

                                printToDoList(ToDoList);
                            }
                        }

                        break;
                    case "4": // Save to-do list to a file
                        try
                        {
                            PersistenceManager.serializeTasks(FILENAME, ToDoList);
                        }
                        catch (Exception ex)
                        {
                            string m = "To-do list could not be saved to the file.\n" + ex.Message;
                            PrintTextWithColor(m, ConsoleColor.Red);
                        }
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
            }
        }

        public void printMainMenu()
        {
            PrintTextWithColor("Welcome to ToDoly!", ConsoleColor.Blue);
            (int numberOfDoneTasks, int numberOfNotDoneTasks) = GetTasksCount();

            string s = numberOfNotDoneTasks < 2 ? "" : "s";
            string isPlural = numberOfDoneTasks < 2 ? " is" : "s are";
            string m = $"You have {numberOfNotDoneTasks} task{s} to do and {numberOfDoneTasks} task{isPlural} done.";

            PrintTextWithColor(m, ConsoleColor.Blue);
            PrintTextWithColor("Pick an option:", ConsoleColor.Blue);
            PrintTextWithColor(Text.APP_MENU, ConsoleColor.Blue);
        }

        public (int, int) GetTasksCount()
        {
            int numberOfDoneTasks = GetTasksByStatus(true).Count;
            int numberOfNotDoneTasks = GetTasksByStatus(false).Count;

            return (numberOfDoneTasks, numberOfNotDoneTasks);
        }

        public void LoadToDoList()
        {
            try
            {
                List<Task>? tasks = PersistenceManager.deserializeTasks(FILENAME);

                if (tasks is null)
                {
                    PrintTextWithColor(Text.DESERIALIZATION_ERROR_NULL, ConsoleColor.Red);
                    Environment.Exit(0);
                }

                ToDoList = tasks;
            }
            catch (JsonException ex)
            {
                PrintTextWithColor(Text.DESERIALIZATION_ERROR_FORMAT + "\n" + ex.Message, ConsoleColor.Red);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                PrintTextWithColor("To-do list could not be loaded." + "\n" + ex.Message, ConsoleColor.Red);
                Environment.Exit(0);
            }
        }

        public string? GetInput()
        {
            string? input = Console.ReadLine();

            if (String.IsNullOrWhiteSpace(input))
                return null;

            input = input!.Trim();

            return input;
        }

        public int? GetInputAsInteger(string input)
        {
            if (!int.TryParse(input, out int number) | number < 1 | number > 3)
            {
                PrintTextWithColor("\nProvide one of these numbers (1, 2 or 3).", ConsoleColor.Red);
                return null;
            }

            return number;
        }

        public bool IsTokenEqual(string str1, string str2)
        {
            return str1.Trim().Equals(str2.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public Task? FindTaskByTitle(string taskTitle)
        {
            return ToDoList.Find(task => IsTokenEqual(task.Title, taskTitle));
        }

        public void printToDoList(List<Task> tasks)
        {
            if (tasks.Count < 1)
                return;

            printHeader();

            foreach (Task task in tasks)
            {
                Console.WriteLine(task);
            }
        }

        public void printTask(Task task)
        {
            printHeader();
            Console.WriteLine(task);
        }

        public void printHeader()
        {
            string title = "TITLE";
            string project = "PROJECT";
            string done = "DONE";
            string header = $"{title.PadRight(35)} {project.PadRight(15)} {done.PadRight(10)} DATE";

            PrintTextWithColor(header, ConsoleColor.Cyan);
        }

        public void GenerateSampleTasks()
        {
            DateTime aDate;
            var today = DateTime.Now;

            for (int i = 1; i < 6; i++)
            {
                aDate = today.AddDays((new Random()).Next(-10, 20));
                Project project = (Project)(new Random()).Next(0, 3);
                bool status = Convert.ToBoolean((new Random()).Next(0, 2));

                ToDoList.Add(new Task { Title = $"Task{i}", DueDate = aDate, Project = project, Status = status });
            }
        }

        public List<Task> SortTaskByDate(List<Task> tasks)
        {
            return tasks.OrderBy(task => task.DueDate).ToList();
        }

        public List<Task> SortTaskByProject(List<Task> tasks)
        {
            return tasks.OrderBy(task => task.Project).ToList();
        }

        public List<Task> GetTasksByStatus(bool done = false)
        {
            return ToDoList.Where(task => task.Status == done).ToList();
        }

        public void PrintTextWithColor(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        static class Text
        {
            public const string APP_MENU = """            
                                                (1) Show Task List (by date or project)
                                                (2) Add New Task
                                                (3) Edit Task (update, mark as done, remove)
                                                (4) Save and Exit
                                            """;

            public const string UPDATE_OPTIONS = """
                                                Enter '1', '2' or '3' to update your task, or else:
                                                    1 = Update title
                                                    2 = Mark as done
                                                    3 = Mark as not done
                                                    4 = Delete task
                                                    5 = Go to main menu
                                                """;

            public const string SORT_OPTIONS = """
                                                Enter '1' or '2' to sort by date or project:
                                                    1 = date
                                                    2 = project
                                                """;

            public const string PROJECT_OPTIONS = """
                                                Enter '1', '2' or '3' to choose a project:
                                                    1 = Home
                                                    2 = Leisure
                                                    3 = Work
                                                """;

            public const string DESERIALIZATION_ERROR_NULL = """
                                                            To-do list could not be loaded.
                                                            Deserialization failed due to null.
                                                            """;

            public const string DESERIALIZATION_ERROR_FORMAT = """
                                                                To-do list could not be loaded.
                                                                Deserialization failed.
                                                                """;

            public const string UPDATE_STATUS = "\nYou have successfully updated your task's status.";

            public const string GO_BACK = "\nEnter 'q' to go back.";
        }
    }

    public class Task
    {
        public string Title { get; set; } = string.Empty;
        public DateTime DueDate { get; init; }
        public bool Status { get; set; }
        public Project Project { get; init; }

        public override string? ToString()
        {
            string status = Status == true ? "Yes" : "No";
            string p = Project.ToString();
            string format = "yyyy-MM-dd";

            return $"{Title.PadRight(35)} {p.PadRight(15)} {status.PadRight(10)} {DueDate.ToString(format)}";
        }
    }

    static class PersistenceManager
    {

        public static void serializeTasks(string fileName, List<Task> tasks)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize<List<Task>>(tasks, options);
            File.WriteAllText(fileName, json);
        }

        public static List<Task>? deserializeTasks(string fileName)
        {
            return JsonSerializer.Deserialize<List<Task>>(File.ReadAllText(fileName));
        }

    }

    public enum Project
    {
        Home,
        Leisure,
        Work
    }
}
