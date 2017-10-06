using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Snake1
    {
        public int Size { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int XSpeed { get; set; }
        public int YSpeed { get; set; }

        public Snake1()
        {
            Size = 3;
            X = 1;
            Y = 1;
            XSpeed = 1;
            YSpeed = 0;
        }

        public void MoveSnake()
        {
            X += XSpeed;
            Y += YSpeed;
        }

        public void ChangeDirection(Directions direction)
        {
            if(direction == Directions.UP)
            {
                if (YSpeed == 1) return;
                XSpeed = 0;
                YSpeed = -1;
            }
            if (direction == Directions.DOWN)
            {
                if (YSpeed == -1) return;
                XSpeed = 0;
                YSpeed = 1;
            }
            if (direction == Directions.LEFT)
            {
                if (XSpeed == 1) return;
                XSpeed = -1;
                YSpeed = 0;
            }
            if (direction == Directions.RIGHT)
            {
                if (XSpeed == -1) return;
                XSpeed = 1;
                YSpeed = 0;
            }
        }

        public enum Directions
        {
            UP,
            DOWN,
            LEFT,
            RIGHT
        }
    }
}
