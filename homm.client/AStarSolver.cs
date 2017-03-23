using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;

namespace Homm.Client
{
    class AStarSolver
    {
        List<Chain> directions;
        MapData map;
        public AStarSolver(MapData map)
        {
            this.map = map;
            directions = new List<Chain>();
            directions.Add(new Chain(-1, -1, ((int)Direction.LeftUp).ToString()));
            directions.Add(new Chain(-1,  1, ((int)Direction.LeftDown).ToString()));
            directions.Add(new Chain( 1, -1, ((int)Direction.RightUp).ToString()));
            directions.Add(new Chain( 1,  1, ((int)Direction.RightDown).ToString()));
            directions.Add(new Chain( 0, -1, ((int)Direction.Up).ToString()));
            directions.Add(new Chain( 0,  1, ((int)Direction.Down).ToString()));
        }

        public struct Place
        {
            public int X;
            public int Y;

            public Place(int ax, int ay)
            {
                X = ax;
                Y = ay;
            }
        }

        // Структура звена
        struct Chain
        {
            public int X;
            public int Y;
            public string path;

            public Chain(int x, int y, string path)
            {
                this.X = x;
                this.Y = y;
                this.path = path;
            }

            public Chain(Place place, string path)
            {
                X = place.X;
                Y = place.Y;
                this.path = path;
            }

            public static Place operator +(Chain current, Chain shift)
            {
                //В зависимости от координаты Х шестигранника current меняется алгоритм перемещения по диагоналям
                //Если мы совершаем движение со сдвигом по координате Х (в сторону)
                if (shift.X !=0) // X==1 || X==-1
                {
                    //в зависимости от текущего положения
                    //не изменяем координату Y, если Х был чётный, а движение совершалось в сторону и вниз
                    //или если X был не чётный, а движение совершалось в сторону и вверх
                    if (current.X % 2 == 0 && shift.Y == 1 || current.X % 2 == 1 && shift.Y == -1)
                    {
                        return new Place(current.X + shift.X, current.Y);
                    }
                }
                return new Place(current.X + shift.X, current.Y + shift.Y);
            }
        }

        public Direction[] GoTo(LocationInfo start, LocationInfo finish)
        {
            Place p_start = new Place(start.X, start.Y);
            Place p_finish = new Place(finish.X, finish.Y);
            string path = ASolverMove(p_start, p_finish);
            Direction[] dir_path = new Direction[path.Length];

            for (int i = 0; i < dir_path.Length; i++)
            {
                switch (path[i])
                {
                    case '0':
                        dir_path[i] = Direction.Up;
                        break;
                    case '1':
                        dir_path[i] = Direction.Down;
                        break;
                    case '2':
                        dir_path[i] = Direction.LeftUp;
                        break;
                    case '3':
                        dir_path[i] = Direction.LeftDown;
                        break;
                    case '4':
                        dir_path[i] = Direction.RightUp;
                        break;
                    case '5':
                        dir_path[i] = Direction.RightDown;
                        break;
                    default:
                        break;
                }
            }
            return dir_path;
        }

        private string ASolverMove(Place start, Place finish)
        {
            Queue<Chain> queue = new Queue<Chain>();
            List<Place> visited_list = new List<Place>();

            queue.Clear();
            visited_list.Clear();

            Chain chain = new Chain(start, String.Empty);

            Place place;

            queue.Enqueue(chain);

            while (queue.Count > 0)
            {
                chain = queue.Dequeue();
                foreach (Chain side in directions)
                {
                    place = chain + side;

                    if (!InRange(place)) //проверка на возможность пройти
                    {
                        continue;
                    }
                    if (visited_list.Contains(place)) //проверка на список проверенных мест
                    {
                        continue;
                    }
                    visited_list.Add(place);

                    //увеличение пути
                    Chain step = new Chain(place, chain.path + side.path);

                    if (place.Equals(finish)) //проверка на достижение цели
                    {
                        return step.path;
                    }
                    queue.Enqueue(step);
                }
            }
            return "-";
        }



        private bool InRange(Place place)
        {
            if (place.X < 0 || place.X >=  map.Width || place.Y < 0 || place.Y >= map.Height)
                return false;

            string barrier = map.Objects.
                Where(x => x.Location.X == place.X && x.Location.Y == place.Y)
                .FirstOrDefault().ToString();

            if (barrier.Equals("Wall") || barrier.StartsWith("Neutral army"))
            {
                return false;
            }
            return true;
        }
    }
}
