using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.IO;

namespace Homm.Client
{
    using MineEnter = HoMM.ClientClasses.Mine;
    using DwellingEnter = HoMM.ClientClasses.Dwelling;
    using Garrison = HoMM.ClientClasses.Garrison;
    using ResourcePile = HoMM.ClientClasses.ResourcePile;
    using NeutralArmy = HoMM.ClientClasses.NeutralArmy;
    using Hero = HoMM.ClientClasses.Hero;
    //Структура Bottom - тип элементов карты недвижимых объектов
    struct Bottom
    {
        public int X;
        public int Y;
        public Place coord;
        public double travelCost;
        public MineEnter mineIsHere;
        public DwellingEnter dwellingIsHere;

        public Bottom(int X, int Y, double travelCost)
        {
            this.X = X;
            this.Y = Y;
            this.travelCost = travelCost;
            coord = new Place(X, Y);
            mineIsHere = null;
            dwellingIsHere = null;
        }

        public Bottom(Place coord, double travelCost) : this(coord.X, coord.Y, travelCost) { }

    }

    struct Top
    {
        public int X;
        public int Y;
        public Place coord;
        public Garrison garrisonIsHere;
        public NeutralArmy neutralArmyIsHere;
        public ResourcePile resourcePileIsHere;
        public Hero heroIsHere;

        public Top(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            coord = new Place(X, Y);
            garrisonIsHere = null;
            neutralArmyIsHere = null;
            resourcePileIsHere = null;
            heroIsHere = null;
        }

        public Top(Place coord) : this(coord.X, coord.Y) { }
    }

    class Vision
    {
        StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\map.txt");

        public Bottom[,] bottom_map;
        //public Top[,] top_map;
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

        private void FillCurrentBottom(int w, int h)
        {
            MapObjectData temp = CurrentObject(w, h);

            
            if (temp.ToString().Equals("Wall")) // Если препятстиве, то стоймость путь равно -1
            {
                bottom_map[w, h].travelCost = -1;
            }
            else
            {
                bottom_map[w, h].travelCost = TileTerrain.Parse(temp.Terrain.ToString()[0]).TravelCost;

                if (temp.ToString().StartsWith("Mine"))
                {
                    bottom_map[w, h].mineIsHere.Owner = temp.Mine.Owner;
                    bottom_map[w, h].mineIsHere.Resource = temp.Mine.Resource;
                }
                else if (temp.ToString().StartsWith("Dwelling"))
                {
                    bottom_map[w, h].dwellingIsHere.Owner = temp.Dwelling.Owner;
                    bottom_map[w, h].dwellingIsHere.UnitType = temp.Dwelling.UnitType;
                    bottom_map[w, h].dwellingIsHere.AvailableToBuyCount = temp.Dwelling.AvailableToBuyCount;
                }
            }
        }

        public void InitMap()
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < widht; w++)
                {
                    if (CurrentObject(w, h) == null)
                    {
                        bottom_map[w, h].travelCost = -2; // Если туман войны - то ставим значение стоймости пути на этой ячейке равной -2
                    }
                    else
                    {
                        FillCurrentBottom(w, h);
                    }
                    sw.Write($"{bottom_map[w, h].travelCost, 5}\t\t");
                }
                sw.WriteLine();
                
            }
            sw.Close();
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
                            FillCurrentBottom(w, h);
                        }
                    }
                }
            }
        }
    }
}
