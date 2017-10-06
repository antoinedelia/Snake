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
            InitGrid();

            Thread thread = new Thread(UpdateGame);
            thread.Start();
        }

        private void InitGrid()
        {
            for (int i = 0; i < Rows; i++)
            {
                var row = new RowDefinition();
                row.Height = new GridLength(50);
                WindowGrid.RowDefinitions.Add(row);
                Grid[i] = new int[Cols];
                for (int j = 0; j < Cols; j++)
                {
                    var col = new ColumnDefinition();
                    col.Width = new GridLength(50);
                    WindowGrid.ColumnDefinitions.Add(col);
                }
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    Canvas canvas = new Canvas();
                    canvas.Background = Brushes.White;
                    canvas.SetValue(System.Windows.Controls.Grid.ColumnProperty, j);
                    canvas.SetValue(System.Windows.Controls.Grid.RowProperty, i);
                    WindowGrid.Children.Add(canvas);
                }
            }
        }

        private void UpdateGame()
        {
            while (true)
            {
                snake.MoveSnake();
                if (IsGameOver()) break;
                SnakeHistory.Add(new Point(snake.X, snake.Y));
                Grid[snake.Y][snake.X] = 1;
                Dispatcher.Invoke(() =>
                {
                    DrawSnake();
                });
                Thread.Sleep(100);
            }
        }

        private void DrawSnake()
        {
            if (SnakeHistory.Count >= snake.Size)
            {
                var itemsInFirstRow = (Canvas)WindowGrid.Children
                    .Cast<UIElement>()
                    .Where(row => System.Windows.Controls.Grid.GetRow(row) == SnakeHistory.ElementAt(0).Y)
                    .Where(col => System.Windows.Controls.Grid.GetColumn(col) == SnakeHistory.ElementAt(0).X)
                    .FirstOrDefault();
                itemsInFirstRow.Background = Brushes.White;
                Grid[(int)SnakeHistory.ElementAt(0).Y][(int)SnakeHistory.ElementAt(0).X] = 0;
                SnakeHistory.RemoveAt(0);
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (Grid[i][j] == 1)
                    {
                        var itemsInFirstRow = (Canvas)WindowGrid.Children
                            .Cast<UIElement>()
                            .Where(row => System.Windows.Controls.Grid.GetRow(row) == i)
                            .Where(col => System.Windows.Controls.Grid.GetColumn(col) == j)
                            .FirstOrDefault();
                        itemsInFirstRow.Background = Brushes.Red;
                    }
                }
            }
        }

        private bool IsGameOver()
        {
            if (snake.X >= Cols || snake.X < 0 || snake.Y >= Rows || snake.Y < 0)
                return true;

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
