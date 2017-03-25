using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.IO;

namespace Homm.Client
{
    class Vision
    {
        public Bottom[,] bottom_map;
        public int widht, height;

        private MapData map;

        public Vision(MapData map)
        {
            this.map = map;
            bottom_map = new Bottom[map.Width, map.Height];
            widht = bottom_map.GetLength(0);
            height = bottom_map.GetLength(1);
        }

        public void InitBottom()
        {
            for (int w = 0; w < widht; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    var type = map.Objects. // Из списка объектов на карте 
                    Where(x => x.Location.X == w && x.Location.Y == h). // находим текущую точку 
                    Select(x => x).
                    FirstOrDefault();

                    if (type.ToString().Equals("Wall"))
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
