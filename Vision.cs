using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.IO;

namespace Homm.Client
{
    //Структура Bottom - тип элементов карты недвижимых объектов
    struct Bottom
    {
        public int X;
        public int Y;
        public Place coord;
        public double travelCost;

        public Bottom(int X, int Y, double travelCost)
        {
            this.X = X;
            this.Y = Y;
            this.travelCost = travelCost;
            coord = new Place(X, Y);
        }

        public Bottom(Place coord, double travelCost) : this(coord.X, coord.Y, travelCost) { }
    }

    struct Top
    {
        public int X;
        public int Y;
        public Place coord;
        List<object> objects;

        public Top(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            coord = new Place(X, Y);
            objects = new List<object>();
        }

        public Top(Place coord) : this(coord.X, coord.Y) { }
    }

    class Vision
    {
        public Bottom[,] bottom_map;
        public Top[,] top_map;
        public int widht, height;

        private MapData map;

        public Vision(MapData map)
        {
            this.map = map;
            bottom_map = new Bottom[map.Width, map.Height];
            widht = bottom_map.GetLength(0);
            height = bottom_map.GetLength(1);
        }

        private MapObjectData CurrentObject(int w, int h)
        {
            return map.Objects. // Из списка объектов на карте 
            Where(x => x.Location.X == w && x.Location.Y == h). // находим текущую точку 
            Select(x => x).
            FirstOrDefault();
        }

        public void InitBottom()
        {
            for (int w = 0; w < widht; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    MapObjectData type = CurrentObject(w, h);

                    if (type == null)
                    {
                        bottom_map[w, h].travelCost = -2; // Если туман войны - то ставим значение стоймости пути на этой ячейке равной -2
                    }
                    else if (type.ToString().Equals("Wall")) // Если препятстиве, то стоймость путь равно -1
                    {
                        bottom_map[w, h].travelCost = -1;
                    }
                    else
                    {
                        bottom_map[w, h].travelCost = TileTerrain.Parse(type.Terrain.ToString()[0]).TravelCost;
                    }
                }
            }
        }

        // Когда происходит открытие нового участка карты, проивзодим
        // обновление карты
        public void UpdateBottom()
        {
            // Ищем позицию героя, и на определенном радиусе от него, перезаписываем карту
            for (int w = 0; w < widht; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    // Если это был туман войны, то проверяем новое значение
                    if (bottom_map[w,h].travelCost == -2)
                    {
                        if (CurrentObject(w,h) != null)
                        {
                            bottom_map[w, h].travelCost = TileTerrain.Parse(CurrentObject(w, h).Terrain.ToString()[0]).TravelCost;
                        }
                    }
                }
            }
        }

        public void InitTop()
        {
            for (int w = 0; w < widht; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    var type = map.Objects. // Из списка объектов на карте 
                    Where(x => x.Location.X == w && x.Location.Y == h). // находим текущую точку 
                    Select(x => x).
                    FirstOrDefault();

                    if (type == null)
                    {
                        bottom_map[w, h].travelCost = -1;
                    }
                    else if (type.ToString().Equals("Wall") /*|| type.ToString().Equals("")*/)
                    {
                        bottom_map[w, h].travelCost = -1;
                    }
                    else
                    {
                        bottom_map[w, h].travelCost = TileTerrain.Parse(type.Terrain.ToString()[0]).TravelCost;
                    }
                }
            }
        }
    }
}
