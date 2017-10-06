using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Snake
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int Cols;
        private int Rows;
        private Snake1 snake;
        private int[][] Grid;
        private List<Point> SnakeHistory;
        public MainWindow()
        {
            InitializeComponent();
            NewGame();
        }

        private void NewGame()
        {
            snake = new Snake1();
            Cols = 20;
            Rows = 20;
            Grid = new int[Rows][];
            SnakeHistory = new List<Point>();
            for (int i = 0; i < Rows; i++)
                Grid[i] = new int[Cols];

            Thread thread = new Thread(UpdateGame);
            thread.Start();
        }

        private void UpdateGame()
        {
            while (true)
            {
                snake.MoveSnake();
                if (IsGameOver()) break;

                if (SnakeHistory.Count >= snake.Size)
                {
                    Grid[(int)SnakeHistory.ElementAt(0).Y][(int)SnakeHistory.ElementAt(0).X] = 0;
                    SnakeHistory.RemoveAt(0);
                }
                SnakeHistory.Add(new Point(snake.X, snake.Y));
                Grid[snake.Y][snake.X] = 1;

                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        if (Grid[i][j] == 1)
                            Console.Write("X");
                        else
                            Console.Write("-");
                    }
                    Console.WriteLine("");
                }
                Console.WriteLine("");
                Thread.Sleep(500);
            }
        }

        private bool IsGameOver()
        {
            if (snake.X >= Cols || snake.X < 0 || snake.Y >= Rows || snake.Y < 0)
            {
                return true;
            }
            return false;
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                if (snake.XSpeed == 1) return;
                snake.XSpeed = -1;
                snake.YSpeed = 0;
            }
            if (e.Key == Key.Right)
            {
                if (snake.XSpeed == -1) return;
                snake.XSpeed = 1;
                snake.YSpeed = 0;
            }
            if (e.Key == Key.Up)
            {
                if (snake.YSpeed == 1) return;
                snake.XSpeed = 0;
                snake.YSpeed = -1;
            }
            if (e.Key == Key.Down)
            {
                if (snake.YSpeed == -1) return;
                snake.XSpeed = 0;
                snake.YSpeed = 1;
            }
        }
    }
}
