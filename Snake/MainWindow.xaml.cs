﻿using System;
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
        private int Cols;
        private int Rows;
        private Snake1 snake;
        private int[][] Grid;
        private List<Point> SnakeHistory;
        private Point FruitPosition;
        private Thread thread;
        private Random random;
        private Controller controller;
        private Gamepad gamepad;
        public int deadband = 2500;
        public Point leftThumb, rightThumb = new Point(0, 0);
        public float leftTrigger, rightTrigger;
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
            random = new Random();
            SnakeHistory = new List<Point>();
            controller = new Controller(UserIndex.One);
            InitGrid();
            MakeFruit();

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
                    if (Grid[i][j] == (int)Case.EMPTY)
                        fruitChoices.Add(new Point(i, j));
                }
            }

            FruitPosition = fruitChoices.ElementAt(random.Next(fruitChoices.Count));
            int x = (int)FruitPosition.X;
            int y = (int)FruitPosition.Y;
            Grid[y][x] = (int)Case.FRUIT;
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
                if (controller.IsConnected) CheckGamePad();
                snake.MoveSnake();
                if (IsGameOver()) break;
                if (snake.X == FruitPosition.X && snake.Y == FruitPosition.Y)
                {
                    MakeFruit();
                    snake.Size++;
                }
                SnakeHistory.Add(new Point(snake.X, snake.Y));
                Grid[snake.Y][snake.X] = 1;
                Dispatcher.Invoke(() =>
                {
                    DrawGame();
                });
                Thread.Sleep(100);
            }
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
                    .Where(row => System.Windows.Controls.Grid.GetRow(row) == SnakeHistory.ElementAt(0).Y)
                    .Where(col => System.Windows.Controls.Grid.GetColumn(col) == SnakeHistory.ElementAt(0).X)
                    .FirstOrDefault();
                itemsInFirstRow.Background = Brushes.White;
                Grid[(int)SnakeHistory.ElementAt(0).Y][(int)SnakeHistory.ElementAt(0).X] = (int)Case.EMPTY;
                SnakeHistory.RemoveAt(0);
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (Grid[i][j] > 0)
                    {
                        var itemsInFirstRow = (Canvas)WindowGrid.Children
                            .Cast<UIElement>()
                            .Where(row => System.Windows.Controls.Grid.GetRow(row) == i)
                            .Where(col => System.Windows.Controls.Grid.GetColumn(col) == j)
                            .FirstOrDefault();
                        if(Grid[i][j] == (int)Case.SNAKE)
                            itemsInFirstRow.Background = Brushes.Red;
                        if (Grid[i][j] == (int)Case.FRUIT)
                            itemsInFirstRow.Background = Brushes.Blue;

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
            if (e.Key == Key.Left) snake.ChangeDirection(Snake1.Directions.LEFT);

            if (e.Key == Key.Right) snake.ChangeDirection(Snake1.Directions.RIGHT);

            if (e.Key == Key.Up) snake.ChangeDirection(Snake1.Directions.UP);
            
            if (e.Key == Key.Down) snake.ChangeDirection(Snake1.Directions.DOWN);
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
