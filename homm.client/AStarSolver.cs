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
        List<Element> directions;
        MapData map;
        public AStarSolver(MapData map)
        {
            this.map = map;
            directions = new List<Element>();
            directions.Add(new Element(-1, -1, ((int)Direction.LeftUp).ToString()));
            directions.Add(new Element(-1,  1, ((int)Direction.LeftDown).ToString()));
            directions.Add(new Element( 1, -1, ((int)Direction.RightUp).ToString()));
            directions.Add(new Element( 1,  1, ((int)Direction.RightDown).ToString()));
            directions.Add(new Element( 0, -1, ((int)Direction.Up).ToString()));
            directions.Add(new Element( 0,  1, ((int)Direction.Down).ToString()));
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
        struct Element
        {
            public int X;
            public int Y;
            public string path;

            public Element(int x, int y, string path)
            {
                this.X = x;
                this.Y = y;
                this.path = path;
            }

            public Element(Place place, string path)
            {
                X = place.X;
                Y = place.Y;
                this.path = path;
            }

            public static Place operator +(Element first, Element second)
            {
                return new Place(first.X + second.X, first.Y + second.Y);
            }
        }

        public string ASolverMove(Place start, Place finish)
        {
            Queue<Element> queue = new Queue<Element>();
            List<Place> visited = new List<Place>();

            queue.Clear();
            visited.Clear();

            Element el = new Element(start, String.Empty);

            Place place;

            queue.Enqueue(el);

            while (queue.Count > 0)
            {
                el = queue.Dequeue();
                foreach (Element side in directions)
                {
                    place = el + side;

                    if (!InRange(place)) //проверка на возможность пройти
                    {
                        continue;
                    }
                    if (visited.Contains(place)) //проверка на список проверенных мест
                    {
                        continue;
                    }
                    visited.Add(place);

                    //увеличение пути
                    Element step = new Element(place, el.path + side.path);

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

            if (map.Objects.
                Where(x => x.Location.X == place.X && x.Location.Y == place.Y)
                .FirstOrDefault().ToString().Equals("Wall"))
            {
                return false;
            }
            return true;
        }
    }
}
