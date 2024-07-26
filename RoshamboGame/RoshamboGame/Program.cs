using TConsole;
using static System.Console;
using static TConsole.ConsoleLibrary;
using System.Drawing;
using System.Data;

namespace RoshamboGame
{
    internal abstract class Program
    {
        private struct GameInfo
        {
            public string Winner;
            public int TotalRounds;
            public int PScore;
            public int CScore;
            public DateTime MatchTime;
        }

        private static readonly string[] MoveOptions = ["Rock", "Paper", "Scissors"];
        private static readonly List<GameInfo> Games = [];
        private const string Path = @"History.txt";
        private static int _computerScore;
        private static int _playerScore;
        private static int _maxRounds = 5;
        private static int _playedRounds;
        private static readonly DataTable GameDataTable = new("GameResults");


        private static void Main()
        {
            OutputEncoding = System.Text.Encoding.UTF8;
            Title = "Roshambo Game";
            Thread loadThread = new(LoadHistory)
            {
                Priority = ThreadPriority.Normal,
                IsBackground = false
            };
            loadThread.Start();
            MainMenu();
            ReadKey(true);
        }

        private static void MainMenu()
        {
            Clear();
            var menu = new TextMenu()
            {
                ShowTitle = true,
                Title = "Roshambo Game".CenterText().BackgroundColor(Color.White).ForegroundColor(Color.Black),
                SelectedItemBackgroundColor = Color.White,
                AutoWrap = false,
                EnableAnimations = false,
                SelectedItemHighlightLength = 20,
                ShowInstructions = false,
                ShowLineUnderMenu = false,
            };
            menu.AddMenuContent("<--[ Are you ready to face the computer ]-->", "Do you think you will beat him?", "Well dive in and lets see");
            menu.AddMenuItems("Play Game", "Game History", "About Me", "Exit");
            switch (menu.Show())
            {
                case 1: NewGame(); break;
                case 2: History(); break;
                case 3: AboutMe(); break;
                case 4: Exit(); break;
            }
        }

        private static void NewGame()
        {
            Clear();
            SetGameRounds();
            var joined = string.Join(" - ", MoveOptions);
            do
            {
                WriteLine(joined.CenterText().BackgroundColor(Color.White).ForegroundColor(Color.Black));
                Write("\n");
                var text = $"Round: {_playedRounds + 1} - Player Score: {_playerScore} - Computer Score: {_computerScore}";
                new BoxBorder()
                {
                    Title = "Status",
                    Message = text,
                    BorderColor = Color.MediumPurple,
                    Style = HighlightStyle.TitleAndMessage
                }.Display();
                var playerMove = AskPlayer();
                var computerMove = MoveOptions[Random.Shared.Next(0, 3)];
                WriteLine($"Computer move is: {computerMove}".ForegroundColor(Color.White));

                switch (playerMove)
                {
                    case "Rock" when computerMove == "Paper": ComputerWin(); break;
                    case "Rock" when computerMove == "Scissors": PlayerWin(); break;

                    case "Paper" when computerMove == "Rock": PlayerWin(); break;
                    case "Paper" when computerMove == "Scissors": ComputerWin(); break;

                    case "Scissors" when computerMove == "Rock": ComputerWin(); break;
                    case "Scissors" when computerMove == "Paper": PlayerWin(); break;

                    default: WriteLine("‼ Draw, no one take this".RgbForeground(251, 210, 68)); break;
                }
                _playedRounds++; Write("\n");
                CountDown("Next Round In: ".Invert(), 3, Color.FromArgb(249, 47, 96));
                if (_playedRounds != _maxRounds)
                {
                    Clear();
                }
                else
                {
                    var pos = CursorTop;
                    CursorTop = 2;
                    var t = $"Round: {_playedRounds} - Player Score: {_playerScore} - Computer Score: {_computerScore}";
                    new BoxBorder()
                    {
                        Title = "Status",
                        Message = t,
                        BorderColor = Color.MediumPurple,
                        Style = HighlightStyle.TitleAndMessage
                    }.Display();
                    CursorTop = pos;
                }
            }
            while (_playedRounds < _maxRounds);
            OnGameDone();
        }

        private static void History()
        {
            CursorVisible = false;
            Clear();
            new Line()
            {
                Title = "Game History",
                Style = LineStyle.Monogram,
                TitleJustify = Justify.Center,
                LineColor = Color.MediumOrchid
            }.Display();
            Write(Environment.NewLine);

            if (Games.Count > 0)
            {
                DisplayDataTable();
            }
            else
            {
                WriteLine("There is no game history yet");
                WriteLine("Go on and play some..");
            }
            ReadKey(true);
            MainMenu();
        }

        private static void AboutMe()
        {
            Clear();
            new Line()
            {
                Title = "About Me".Invert(),
                TitleJustify= Justify.Center,
                LineColor = Color.FromArgb(104, 195, 212),
            }.Display();
            Write(Environment.NewLine);
            WriteLine("Hi, My name is Abodi Dawoud");
            WriteLine("Im 23 years old and living in Stockholm with my family for nearly 10 years ago");
            WriteLine("I like to develop and write codes and especially for consoles application");
            WriteLine("I hope you like my work..");
            Write("\n");
            PressAnyKey(null);
            MainMenu();
        }

        private static void Exit()
        {
            Clear();
            var menu = new ConfirmMenu("You are about to exit the game, Are you sure?")
            {
                TitleColor = Color.White,
                SelectedItemBackgroundColor = Color.White,
                ShowLineUnderTitle = true,
                DisplayWay = EDisplayWay.Horizontal,
                LineColor = Color.HotPink,
            };
            if (menu.Show()) Environment.Exit(0);
            MainMenu();
        }

        private static string AskPlayer()
        {
            var promptOptions = new Prompt(PromptType.String)
            {
                Message = "Enter your move: ",
                AcceptSpace = false,
                MinimumChars = 0,
                MaximumChars = 8,
                RemovePromptWhenDone = false,
                Prefix = PrefixType.None,
            };
            var prompt = promptOptions.Display().ToTitleCase();

            while (!MoveOptions.Contains(prompt))
            {
                CursorTop--;
                Write("\r\u001b[2K");
                prompt = promptOptions.Display().ToTitleCase();
            }
            return prompt;
        }

        private static void SetGameRounds()
        {
            const string msg = "How many rounds whould you like to play in this match: ";
            Write("\u25C9 ".RgbForeground(247, 37, 133));
            ConsoleAnimations.WriteLine(msg, null, 38);
            var answer = new Prompt(PromptType.Numeric)
            {
                AcceptSpace = false,
                MinimumChars = 0,
                MaximumChars = 2,
                Prefix = PrefixType.None,
                Message = string.Empty,
                RemovePromptWhenDone = false,
                InputColor = Color.FromArgb(84, 138, 247),
            }.Display();
            _maxRounds = Convert.ToInt32(answer);
            CursorTop--; CursorLeft = msg.Length + 3;
            ConsoleAnimations.RemoveLine(0, RemoveCase.Backward, 25);
        }

        private static void PlayerWin()
        {
            _playerScore++;
            WriteLine("✓ You win this".RgbForeground(0, 210, 106));
        }

        private static void ComputerWin()
        {
            _computerScore++;
            WriteLine("✕ Computer win this".RgbForeground(249, 47, 96));
        }

        private static void OnGameDone()
        {
            var game = new GameInfo
            {
                TotalRounds = _maxRounds,
                PScore = _playerScore,
                CScore = _computerScore,
                MatchTime = DateTime.Now, //.ToString("yyyy/MM/dd - HH:mm"),
                Winner = _playerScore > _computerScore ? "Player" : _computerScore == _playerScore ? "No One" : "Computer"
            };
            Games.Add(game);
            var line = $"{DateTime.Now} | {_maxRounds} | {_playerScore} | {_computerScore} | {game.Winner}";
            ConsoleIO.AddLineToFile(Path, line);
            ResetGame();
            GameDoneMenu();
        }

        private static void ResetGame()
        {
            _maxRounds = 0;
            _playerScore = 0;
            _computerScore = 0;
            _playedRounds = 0;
        }

        private static void GameDoneMenu()
        {
            new Line()
            {
                LineColor = Color.MediumPurple
            }.Display();
            var menu = new TextMenu
            {
                Title = "Game is done..",
                AutoWrap = true,
                EnableAnimations = true,
                AnimationSpeed = 5,
                ShowTitle = true,
                ShowInstructions = false,
                SelectedItemBackgroundColor = Color.White
            };
            menu.AddMenuItems("Play again", "History Log", "Main Menu");
            switch (menu.Show())
            {
                case 1: NewGame(); break;
                case 2: History(); break;
                case 3: MainMenu(); break;
            }
        }

        private static void LoadHistory()
        {
            if (!File.Exists(Path)) return;
            foreach (var line in ConsoleIO.GetAllLinesFromFile(Path))
            {
                var splited = line.Split('|', options: StringSplitOptions.TrimEntries);
                var info = new GameInfo()
                {
                    MatchTime = Convert.ToDateTime(splited[0]),
                    TotalRounds = Convert.ToInt32(splited[1]),
                    PScore = Convert.ToInt32(splited[2]),
                    CScore = Convert.ToInt32(splited[3]),
                    Winner = splited[4],
                };
                Games.Add(info);
            }
        }

        private static void DisplayDataTable()
        {    
            // Add columns to the DataTable
            if (GameDataTable.Columns.Count <= 0)
            {
                GameDataTable.Columns.Add("ID", typeof(int));
                GameDataTable.Columns.Add("GameTime", typeof(DateTime));
                GameDataTable.Columns.Add("TotalRounds", typeof(int));
                GameDataTable.Columns.Add("PlayerScore", typeof(int));
                GameDataTable.Columns.Add("ComputerScore", typeof(int));
                GameDataTable.Columns.Add("Winner", typeof(string));
            }


            // Add rows to the DataTable from the games list
            var id = 1;
            foreach (var game in  Games)
            {
                GameDataTable.Rows.Add(id, game.MatchTime, game.TotalRounds, game.PScore, game.CScore, game.Winner);
                id++;
            }
            
            // Write the columns
            WriteLine($"{"ID",-8}{"GameTime",-25}{"TotalRounds",-25}{"PlayerScore",-25}{"ComputerScore",-25}{"Winner"}");

            // Line Seperator
            WriteLine(new string('\u2500', WindowWidth));

            
            for (var i = 0; i < GameDataTable.Columns.Count; i++)
            {
                Write("");
            }
            

            // Write the rows
            foreach (DataRow row in GameDataTable.Rows)
            {
                WriteLine($"{row["ID"],-8}{(DateTime)row["GameTime"],-25:yyyy-MM-dd HH:mm}{row["TotalRounds"],-25}{row["PlayerScore"],-25}{row["ComputerScore"],-25}{row["Winner"]}");
            }
        }
    }
}