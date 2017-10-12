using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using SharpDX.XInput;

namespace Snake
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static int SIZE = 40;
        private int Cols;
        private int Rows;
        private int Score;
        private bool GameOver;
        private Snake1 snake;
        private int[][] GameGrid;
        private List<Point> SnakeHistory;
        private Point FruitPosition;
        private Thread thread;
        private Random random;
        private Controller controller;
        private Gamepad gamepad;
        public int deadband = 2500;
        public Point leftThumb, rightThumb = new Point(0, 0);
        public MainWindow()
        {
            InitializeComponent();
            NewGame();
        }

        private void NewGame()
        {
            Cols = 20;
            Rows = 20;
            Score = 0;
            Height = Rows * SIZE + 150;
            Width = Cols * SIZE + 100;
            GameOver = false;
            GameGrid = new int[Rows][];
            random = new Random();
            SnakeHistory = new List<Point>();
            InitGrid();
            controller = new Controller(UserIndex.One);
            MakeFruit();
            snake = new Snake1();

            thread = new Thread(UpdateGame);
            thread.Start();
        }

        private void MakeFruit()
        {
            List<Point> fruitChoices = new List<Point>();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (GameGrid[i][j] == (int)Case.EMPTY)
                        fruitChoices.Add(new Point(j, i));
                }
            }

            FruitPosition = fruitChoices.ElementAt(random.Next(fruitChoices.Count));
            int x = (int)FruitPosition.X;
            int y = (int)FruitPosition.Y;
            GameGrid[y][x] = (int)Case.FRUIT;
        }

        private void InitGrid()
        {
            ClearGrid();
            for (int i = 0; i < Rows; i++)
            {
                var row = new RowDefinition();
                row.Height = new GridLength(SIZE);
                WindowGrid.RowDefinitions.Add(row);
                GameGrid[i] = new int[Cols];
                for (int j = 0; j < Cols; j++)
                {
                    var col = new ColumnDefinition();
                    col.Width = new GridLength(SIZE);
                    WindowGrid.ColumnDefinitions.Add(col);
                }
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    Canvas canvas = new Canvas();
                    canvas.Background = Brushes.White;
                    canvas.SetValue(Grid.ColumnProperty, j);
                    canvas.SetValue(Grid.RowProperty, i);
                    WindowGrid.Children.Add(canvas);
                }
            }

            //Add score
            var row2 = new RowDefinition();
            row2.Height = new GridLength(SIZE);
            WindowGrid.RowDefinitions.Add(row2);
            var score = new TextBox()
            {
                Text = "Score : " + Score.ToString(),
                FontSize = 20,
                Foreground = Brushes.White,
                Background = Brushes.Black
            };
            score.SetValue(Grid.RowProperty, Rows + 1);
            score.SetValue(Grid.ColumnProperty, 0);
            Grid.SetColumnSpan(score, Cols/2);
            WindowGrid.Children.Add(score);
        }

        private void ClearGrid()
        {
            WindowGrid.Children.Clear();
            WindowGrid.RowDefinitions.Clear();
            WindowGrid.ColumnDefinitions.Clear();
        }

        private void UpdateGame()
        {
            while (!GameOver)
            {
                if (controller.IsConnected) CheckGamePad();
                snake.MoveSnake();
                if (IsGameOver())
                {
                    GameOver = true;
                    break;
                }
                if (snake.X == FruitPosition.X && snake.Y == FruitPosition.Y)
                {
                    MakeFruit();
                    Dispatcher.Invoke(() =>
                    {
                        UpdateScore();
                    });
                    snake.Size++;
                }
                SnakeHistory.Add(new Point(snake.X, snake.Y));
                GameGrid[snake.Y][snake.X] = 1;
                Dispatcher.Invoke(() =>
                {
                    DrawGame();
                });
                Thread.Sleep(100);
            }
        }

        private void UpdateScore()
        {
            Score++;
            var score = (TextBox)WindowGrid.Children
              .Cast<UIElement>()
              .First(e => Grid.GetRow(e) == Rows + 1 && Grid.GetColumn(e) == 0);
            score.Text = "Score : " + Score.ToString();

        }

        private void CheckGamePad()
        {
            gamepad = controller.GetState().Gamepad;
            leftThumb.X = (Math.Abs((float)gamepad.LeftThumbX) < deadband) ? 0 : (float)gamepad.LeftThumbX / short.MinValue * -100;
            leftThumb.Y = (Math.Abs((float)gamepad.LeftThumbY) < deadband) ? 0 : (float)gamepad.LeftThumbY / short.MaxValue * 100;

            if (leftThumb.X < -50) snake.ChangeDirection(Snake1.Directions.LEFT);

            if (leftThumb.X > 50) snake.ChangeDirection(Snake1.Directions.RIGHT);

            if (leftThumb.Y < -50) snake.ChangeDirection(Snake1.Directions.DOWN);

            if (leftThumb.Y > 50) snake.ChangeDirection(Snake1.Directions.UP);
        }

        private void DrawGame()
        {
            if (SnakeHistory.Count > snake.Size)
            {
                var itemsInFirstRow = (Canvas)WindowGrid.Children
                    .Cast<UIElement>()
                    .Where(row => Grid.GetRow(row) == SnakeHistory.ElementAt(0).Y)
                    .Where(col => Grid.GetColumn(col) == SnakeHistory.ElementAt(0).X)
                    .FirstOrDefault();
                itemsInFirstRow.Background = Brushes.White;
                GameGrid[(int)SnakeHistory.ElementAt(0).Y][(int)SnakeHistory.ElementAt(0).X] = (int)Case.EMPTY;
                SnakeHistory.RemoveAt(0);
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (GameGrid[i][j] > 0)
                    {
                        var itemsInFirstRow = (Canvas)WindowGrid.Children
                            .Cast<UIElement>()
                            .Where(row => Grid.GetRow(row) == i)
                            .Where(col => Grid.GetColumn(col) == j)
                            .FirstOrDefault();
                        if (GameGrid[i][j] == (int)Case.SNAKE)
                            itemsInFirstRow.Background = Brushes.Red;
                        if (GameGrid[i][j] == (int)Case.FRUIT)
                            itemsInFirstRow.Background = Brushes.Blue;

                    }
                }
            }
        }

        private bool IsGameOver()
        {
            if (snake.X >= Cols || snake.X < 0 || snake.Y >= Rows || snake.Y < 0)
                return true;
            foreach (var item in SnakeHistory)
            {
                if (snake.X == item.X && snake.Y == item.Y)
                    return true;
            }
            return false;
        }

        private void window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left) snake.ChangeDirection(Snake1.Directions.LEFT);

            if (e.Key == Key.Right) snake.ChangeDirection(Snake1.Directions.RIGHT);

            if (e.Key == Key.Up) snake.ChangeDirection(Snake1.Directions.UP);

            if (e.Key == Key.Down) snake.ChangeDirection(Snake1.Directions.DOWN);

            if (e.Key == Key.R && GameOver) NewGame();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            thread.Abort();
            base.OnClosing(e);
        }

        public enum Case
        {
            EMPTY = 0,
            SNAKE = 1,
            FRUIT = 2
        }
    }
}
