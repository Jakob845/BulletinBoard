using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace BulletinBoard
{

    public class User
    {
        public int ID { get; set; }
        [Required]
        [MaxLength(255)]
        public string UserName { get; set; }
        [Required]
        [MaxLength(255)]
        public string Password { get; set; }

        public List<Bulletin> Bulletins { get; set; }
    }

    public class Bulletin
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public DateTime DateTime { get; set; }
        public User Author { get; set; }
        public string Category { get; set; }
        public int UpVote { get; set; }
    }

    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Bulletin> Bulletins { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
            .HasIndex(u => u.UserName)
            .IsUnique();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(@"Data Source=(local)\SQLEXPRESS;Initial Catalog=BulletinBoardDB;Integrated Security=True");
        }
    }

    public class Program
    {
        private static AppDbContext database;
        private static string loggedIn = "";

        public static void Main()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            using (database = new AppDbContext())
            {
                Utils.WriteHeading("Welcome to the Bulletin Board");

                bool running = true;
                while (running)
                {
                    int selected = Utils.ShowMenu("What do you want to do?", new[] {
                    "Sign in",
                    "Create account",
                    "Exit"
                });
                    Console.Clear();

                    if (selected == 0) SignIn();
                    else if (selected == 1) CreateAccount();
                    else running = false;

                    Console.WriteLine();
                }
                Console.WriteLine("Good bye! Come back soon!");
                Console.WriteLine("Press any key to close the window");
                Console.ReadKey();
            }
        }
        public static void SignIn()
        {
            Utils.WriteHeading("Sign in");
            var userName = Utils.ReadString("User Name: ");
            var password = Utils.ReadString("Password: ");

            bool InLoggIsCorrect = false;
            foreach (var user in database.Users)
            {
                if (userName == user.UserName && password == user.Password)
                {
                    InLoggIsCorrect = true;
                }
            }

            if (InLoggIsCorrect)
            {
                Console.Clear();
                Console.WriteLine("Hello " + userName + "! ");
                Console.WriteLine();
                loggedIn = userName;
                SignedInMenu();
            }
            else
            {
                Console.Clear();
                Console.WriteLine("The Username or password was incorrect");
            }

        }

        public static void CreateAccount()
        {
            Utils.WriteHeading("Create account");
            User user = new User();
            var userName = Utils.ReadString("User Name: ");
            user.UserName = userName;
            var password = Utils.ReadString("Password: ");
            user.Password = password;

            database.Add(user);
            try
            {
                database.SaveChanges();

                Console.Clear();
                Console.WriteLine("Hello " + userName + "! Your account has been created!");
            }
            catch
            {
                Console.Clear();
                Console.WriteLine("That user already exist.");
            }

        }

        public static void SignedInMenu()
        {
            bool running = true;
            while (running)
            {
                int selected = Utils.ShowMenu("What do you want to do?", new[] {
                    "List all the latest bulletins!",
                    "List all the latest bulletins from a category",
                    "List Bulletins by Likes",
                    "Create a new bulletin",
                    "Sign out"
                });
                Console.Clear();

                if (selected == 0) ListBulletins();
                else if (selected == 1) ListBulletinsByCategory();
                else if (selected == 2) ListMostePopularBulletins();
                else if (selected == 3) CreateNewBulletin();
                else
                {
                    running = false;
                    loggedIn = "";
                    Console.Write("You have been signed out");
                }

                Console.WriteLine();
            }
        }

        public static void ListBulletins()
        {
            Utils.WriteHeading("Latest bulletins: ");
            if (database.Bulletins.Count() == 0)
            {
                Console.WriteLine("There are no bulletins to show");
            }
            else
            {
                List<string> tempStringList = new List<string>();

                foreach (var bulletin in database.Bulletins.Include(u => u.Author).OrderBy(b => b.DateTime))
                {
                    string tempString = bulletin.ID + "#" + "Category: " + bulletin.Category + "\n" +
                        bulletin.DateTime + "\n" +
                        "Written by " + bulletin.Author.UserName + "\n\n" +
                        bulletin.Text + "\n" + "Likes: " + bulletin.UpVote + "\n" + "------------------";
                    tempStringList.Add(tempString);
                }
                var tempStringArray = tempStringList.ToArray();
                int selectedBulletin = Utils.ShowMenu("", tempStringArray);
                if (selectedBulletin != -1)
                {
                    int index = tempStringArray[selectedBulletin].IndexOf("#");
                    int bulletinID = Convert.ToInt32(tempStringArray[selectedBulletin].Substring(0, index));

                    var bullet = database.Bulletins.Single(i => i.ID == bulletinID);
                    bullet.UpVote += 1;
                    database.SaveChanges();
                    Console.WriteLine("You liked " + bullet.Author.UserName + "s bulletin.");

                    Console.WriteLine();
                    Console.WriteLine("Press any key to go back to the menu.");
                    Console.ReadKey();
                }
                Console.Clear();
            }
        }

        public static void ListBulletinsByCategory()
        {
            if (database.Bulletins.Count() == 0)
            {
                Console.WriteLine("There are no bulletins to show");
            }
            else
            {
                Utils.WriteHeading("Pick a category: ");

                List<string> tempString = new List<string>();
                try
                {


                    foreach (var b in database.Bulletins)
                    {
                        tempString.Add(b.Category);
                    }

                    var tempStringArray = tempString.Distinct().ToArray();
                    int selectedArrayIndex = Utils.ShowMenu("From which category do you want to get bulletins?", tempStringArray);
                    string selectedCategory = tempStringArray[selectedArrayIndex];
                    Console.Clear();
                    if (selectedArrayIndex != -1)
                    {
                        Utils.WriteHeading(selectedCategory);

                        List<string> tempStringList = new List<string>();

                        foreach (var bulletin in database.Bulletins.Where(c => c.Category == selectedCategory).Include(u => u.Author).OrderBy(b => b.DateTime))
                        {
                            string temporaryString = bulletin.ID + "#" + "Category: " + bulletin.Category + "\n" +
                            bulletin.DateTime + "\n" +
                            "Written by " + bulletin.Author.UserName + "\n\n" +
                            bulletin.Text + "\n" + "Likes: " + bulletin.UpVote + "\n" + "------------------";
                            tempStringList.Add(temporaryString);
                        }
                        var temporaryStringArray = tempStringList.ToArray();
                        int selectedBulletin = Utils.ShowMenu("", temporaryStringArray);
                        if (selectedBulletin != -1)
                        {
                            int index = temporaryStringArray[selectedBulletin].IndexOf("#");
                            int bulletinID = Convert.ToInt32(temporaryStringArray[selectedBulletin].Substring(0, index));

                            var bullet = database.Bulletins.Single(i => i.ID == bulletinID);
                            bullet.UpVote += 1;
                            database.SaveChanges();
                            Console.WriteLine("You liked " + bullet.Author.UserName + "s bulletin.");

                            Console.WriteLine();
                            Console.WriteLine("Press any key to go back to the menu.");
                            Console.ReadKey();
                        }
                        Console.Clear();
                    }
                }
                catch { Console.Clear(); Console.WriteLine("There are no categories yet"); }

            }
        }

        public static void ListMostePopularBulletins()
        {
            Utils.WriteHeading("Latest bulletins: ");
            if (database.Bulletins.Count() == 0)
            {
                Console.WriteLine("There are no bulletins to show");
            }
            else
            {
                List<string> tempStringList = new List<string>();

                foreach (var bulletin in database.Bulletins.Include(u => u.Author).OrderBy(b => b.UpVote).ThenBy(b => b.DateTime))
                {
                    string tempString = bulletin.ID + "#" + "Category: " + bulletin.Category + "\n" +
                        bulletin.DateTime + "\n" +
                        "Written by " + bulletin.Author.UserName + "\n\n" +
                        bulletin.Text + "\n" + "Likes: " + bulletin.UpVote + "\n" + "------------------";
                    tempStringList.Add(tempString);
                }
                var tempStringArray = tempStringList.ToArray();
                int selectedBulletin = Utils.ShowMenu("", tempStringArray);
                if (selectedBulletin != -1)
                {
                    int index = tempStringArray[selectedBulletin].IndexOf("#");
                    int bulletinID = Convert.ToInt32(tempStringArray[selectedBulletin].Substring(0, index));

                    var bullet = database.Bulletins.Single(i => i.ID == bulletinID);
                    bullet.UpVote += 1;
                    database.SaveChanges();
                    Console.WriteLine("You liked " + bullet.Author.UserName + "s bulletin.");

                    Console.WriteLine();
                    Console.WriteLine("Press any key to go back to the menu.");
                    Console.ReadKey();
                }
                Console.Clear();
            }
        }

        public static void CreateNewBulletin()
        {
            Utils.WriteHeading("New bulletin");
            Bulletin bulletin = new Bulletin();
            bulletin.Author = database.Users.Single(i => i.UserName == loggedIn);
            bulletin.DateTime = DateTime.Now;


            int selected = Utils.ShowMenu("Do you want to write your own category or select one that already exist?", new[] {
                    "Write my own",
                    "Pick from list"
                });
            Console.Clear();

            if (selected != -1)
            {
                if (selected == 0) bulletin.Category = Utils.ReadString("Category: ");
                else if (selected == 1)
                {
                    List<string> tempString = new List<string>();
                    try
                    {
                        foreach (var b in database.Bulletins)
                        {
                            tempString.Add(b.Category);
                        }
                        var tempStringArray = tempString.Distinct().ToArray();
                        int indexTemmpStringArray = Utils.ShowMenu("Pick a Category: ", tempStringArray);
                        if (indexTemmpStringArray != -1)
                        {
                            bulletin.Category = tempString[indexTemmpStringArray];
                        }
                    }
                    catch
                    {
                        Console.Clear();
                        Console.WriteLine("There are no categories yet.");
                    }

                }
                if (bulletin.Category != null)
                {


                    bulletin.Text = Utils.ReadString("Bulletin text: ");
                    database.Add(bulletin);
                    database.SaveChanges();
                    Console.Clear();
                    Console.WriteLine("Your bulletin is now on the board");
                }
                else { Console.Clear(); }
            }

        }

        public static class Utils
        {

            public static string ReadString(string prompt)
            {
                Console.Write(prompt + " ");
                string input = Console.ReadLine();
                return input;
            }

            public static void WriteHeading(string text)
            {
                Console.WriteLine(text);
                string underline = new string('-', text.Length);
                Console.WriteLine(underline);
            }

            public static int ShowMenu(string prompt, string[] options)
            {
                if (options == null || options.Length == 0)
                {
                    throw new ArgumentException("Cannot show a menu for an empty array of options.");
                }

                Console.WriteLine(prompt);

                int selected = 0;

                // Hide the cursor that will blink after calling ReadKey.
                Console.CursorVisible = false;

                ConsoleKey? key = null;
                while (key != ConsoleKey.Enter)
                {
                    // If this is not the first iteration, move the cursor to the first line of the menu.
                    if (key != null)
                    {
                        int numLines = 0;
                        foreach (var option in options)
                        {
                            numLines = option.ToString().Split('\n').Length;
                        }
                        Console.CursorLeft = 0;
                        Console.CursorTop = Console.CursorTop - options.Length * numLines;
                        if (numLines > 1)
                            Console.SetWindowPosition(0, selected * numLines);
                    }

                    // Print all the options, highlighting the selected one.
                    for (int i = 0; i < options.Length; i++)
                    {
                        var option = options[i];
                        option = option.Substring(option.IndexOf('#') + 1);
                        if (i == selected)
                        {
                            Console.BackgroundColor = ConsoleColor.Blue;
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Console.WriteLine("- " + option);
                        Console.ResetColor();
                    }

                    // Read another key and adjust the selected value before looping to repeat all of this.
                    key = Console.ReadKey().Key;
                    if (key == ConsoleKey.DownArrow)
                    {
                        selected = Math.Min(selected + 1, options.Length - 1);
                    }
                    else if (key == ConsoleKey.UpArrow)
                    {
                        selected = Math.Max(selected - 1, 0);
                    }
                    else if (key == ConsoleKey.Backspace)
                    {
                        Console.Clear();
                        selected = -1;
                        break;
                    }

                    //Console.SetWindowPosition(0, selected);
                }

                // Reset the cursor and return the selected option.
                Console.CursorVisible = true;
                return selected;
            }

        }
    }
}
