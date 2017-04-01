using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.IO;

namespace Homm.Client
{
    // Определение однозначности в используемых классах
    using MineEnter = HoMM.ClientClasses.Mine;
    using DwellingEnter = HoMM.ClientClasses.Dwelling;
    using Garrison = HoMM.ClientClasses.Garrison;
    using ResourcePile = HoMM.ClientClasses.ResourcePile;
    using NeutralArmy = HoMM.ClientClasses.NeutralArmy;
    using Hero = HoMM.ClientClasses.Hero;

    #region structs

    // Структура Bottom - тип элементов карты недвижимых объектов
    struct Bottom
    {
        public int X; // Позиция по оси Х
        public int Y; // Позиция по оси Y
        public Place coord; // Поле Place, записывающее координаты X, Y
        public double travelCost; // Время передвижения по данной ячейке

        // Конструктор
        public Bottom(int X, int Y, double travelCost)
        {
            this.X = X;
            this.Y = Y;
            this.travelCost = travelCost;
            coord = new Place(X, Y);
        }

        // Перегруженный конструктор
        public Bottom(Place coord, double travelCost) : this(coord.X, coord.Y, travelCost) { }

    }

    struct TopItemPlaces
    {
        public Place coord;
        public MineEnter mineIsHere;
        public DwellingEnter dwellingIsHere;
        public ResourcePile resourceIsHere;
        public NeutralArmy neutralArmyIsHere;

        //конструктор, инициализирует, как шахту
        public TopItemPlaces(Place coord, MineEnter mineIsHere)
        {
            this.coord = coord;
            this.mineIsHere = mineIsHere;

            this.mineIsHere.Owner = mineIsHere.Owner?.ToString() ?? "Neutral";
            this.dwellingIsHere = null;
            this.resourceIsHere = null;
            this.neutralArmyIsHere = null;
        }

        //конструктор, инициализирует, как таверну
        public TopItemPlaces(Place coord, DwellingEnter dwellingIsHere)
        {
            this.coord = coord;
            this.mineIsHere = null;
            this.dwellingIsHere = dwellingIsHere;
            this.dwellingIsHere.Owner = dwellingIsHere.Owner?.ToString() ?? "Neutral";
            this.resourceIsHere = null;
            this.neutralArmyIsHere = null;

        }

        //конструктор, инициализирует, как ресурс
        public TopItemPlaces(Place coord, ResourcePile resourceIsHere)
        {
            this.coord = coord;
            this.mineIsHere = null;
            this.dwellingIsHere = null;
            this.resourceIsHere = resourceIsHere;
            this.neutralArmyIsHere = null;

        }

        // конструктор, инициализирует, как нейтральную армию
        public TopItemPlaces(Place coord, NeutralArmy neutralArmyIsHere)
        {
            this.coord = coord;
            this.mineIsHere = null;
            this.dwellingIsHere = null;
            this.resourceIsHere = null;
            this.neutralArmyIsHere = neutralArmyIsHere;

        }

        public bool Equals(Place pl)
        {
            return coord.X == pl.X && coord.Y == pl.Y;
        }

    }

    #endregion

    class Vision
    {
        // Запись карты 
        StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\map.txt");

        public Bottom[,] bottom_map; // Карта недвижимых объектов 
        public int widht, height; // Длина и ширина карты

        public List<TopItemPlaces> mines = new List<TopItemPlaces>();
        public List<TopItemPlaces> dwellings = new List<TopItemPlaces>();
        public List<TopItemPlaces> resources = new List<TopItemPlaces>();
        public List<TopItemPlaces> neutralarmies = new List<TopItemPlaces>();

        private MapData map; // Полная карта мира

        // Констркутор
        public Vision(MapData map)
        {
            this.map = map;
            bottom_map = new Bottom[map.Width, map.Height];

            // Обязательно инициализируем каждый элемент массива
            for (int i = 0; i < map.Width; i++)
            {
                for (int j = 0; j < map.Height; j++)
                {
                    bottom_map[i, j] = new Bottom(i, j, 0);
                }
            }

            widht = bottom_map.GetLength(0);
            height = bottom_map.GetLength(1);
        }

        // Метод возвращающий необходимую клетку игрового мира по посылаемым координатам
        private MapObjectData CurrentObject(int w, int h)
        {
            return map.Objects. // Из списка объектов на карте 
            Where(x => x.Location.X == w && x.Location.Y == h). // находим текущую точку 
            Select(x => x).
            FirstOrDefault();
        }

        private MapObjectData CurrentObject(Place coord) => CurrentObject(coord.X, coord.Y);

        // Метод, заносящий посылаемую клетку в карту Bottom
        private void FillCurrentBottom(int w, int h)
        {
            MapObjectData temp = CurrentObject(w, h);

            // Если препятстиве, то стоймость пути равна -1
            if (temp.ToString().Equals("Wall"))
            {
                bottom_map[w, h].travelCost = -1;
            }
            else
            {
                // Записываем стоймость пути на данной клетке
                bottom_map[w, h].travelCost = TileTerrain.Parse(temp.Terrain.ToString()[0]).TravelCost;

                // Если на данной клетке находится вход в шахту
                if (temp.Mine != null)
                {
                    // Записываем его в список шахт
                    mines.Add(new TopItemPlaces(new Place(w, h), temp.Mine));
                }
                // Иначе если на данной клетке находится вход в пункт найма юнитов
                else if (temp.Dwelling != null)
                {
                    // Записываем его в список таверн
                    dwellings.Add(new TopItemPlaces(new Place(w, h), temp.Dwelling));
                }
                // Иначе если на данной клетке находится ресурс
                else if (temp.ResourcePile != null)
                {
                    // Записываем его в список ресурсов
                    resources.Add(new TopItemPlaces(new Place(w, h), temp.ResourcePile));
                }
                // Иначе если на данной клетке находится нейтральная армия
                else if (temp.NeutralArmy != null)
                {
                    // Записываем ее в список нейтральных армий
                    neutralarmies.Add(new TopItemPlaces(new Place(w, h), temp.NeutralArmy));
                }
            }
        }
        
        public void InitTextMap()
        {
            Bottom[,] local_map = new Bottom[widht, height];

            sw.Write(new String(' ', 3));
            for (int w = 0; w < widht; w++)
            {
                sw.Write($"{w}\t\t");

            }
            sw.WriteLine();
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < widht; w++)
                {
                    if(w==0)
                        sw.Write(h.ToString(), 3);
                    // Если туман войны - то ставим значение стоймости пути на этой ячейке равной -2
                    if (CurrentObject(w, h) == null)
                    {
                        local_map[w, h].travelCost = -2;
                    }
                    else
                    {
                        FillCurrentBottom(w, h);
                        //FillCurrentTop(w, h);
                    }
                    sw.Write($"{bottom_map[w, h].travelCost, 6}\t\t");
                }
                sw.WriteLine();
            }
            sw.Close();
        }

        // Начальное заполнение карт Bottom и Top
        public void InitMap()
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < widht; w++)
                {
                    // Если туман войны - то ставим значение стоймости пути на этой ячейке равной -2
                    if (CurrentObject(w, h) == null)
                    {
                        bottom_map[w, h].travelCost = -2;
                    }
                    else
                    {
                        FillCurrentBottom(w, h);
                    }
                }
            }
        }

        #region Updates

        // Метод Update, каждый раз получая данные с сервера обновляем наши списки
        // и карту
        public void Update(HommSensorData data)
        {
            map = data.Map;
            UpdateBottom();
            UpdateDwellings();
            UpdateMines();
            UpdateNeutrals();
            UpdateResource();
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
                    if (bottom_map[w, h].travelCost == -2)
                    {
                        if (CurrentObject(w, h) != null)
                        {
                            FillCurrentBottom(w, h);
                        }
                    }
                }
            }
        }

        public void UpdateMines()
        {
            for (int i = 0; i < mines.Count; i++)
            {
                var temp = CurrentObject(mines[i].coord).Mine.Owner?.ToString() ?? "Neutral";

                if (!temp.Equals(mines[i].mineIsHere.Owner))
                {
                    mines[i].mineIsHere.Owner = temp;
                }
            }

        }
        public void UpdateDwellings()
        {
            for (int i = 0; i < dwellings.Count; i++)
            {
                var temp = CurrentObject(dwellings[i].coord).Dwelling.Owner?.ToString() ?? "Neutral";

                if (!temp.Equals(dwellings[i].dwellingIsHere.Owner))
                {
                    dwellings[i].dwellingIsHere.Owner = temp;
                }
            }
        }

        public void UpdateResource()
        {
            for (int i = 0; i < resources.Count; i++)
            {
                var temp_1 = CurrentObject(resources[i].coord).ResourcePile;
                //var temp = data.Map;
                //temp.OnSensorDataReceived;

                if (temp_1 == null)
                {
                    resources.RemoveAt(i);
                    i--;
                }
            }
            //UpdateResource();
        }

        public void UpdateNeutrals()
        {
            for (int i = 0; i < neutralarmies.Count; i++)
            {
                var temp = CurrentObject(neutralarmies[i].coord).NeutralArmy;

                if (temp == null)
                {
                    neutralarmies.RemoveAt(i);
                }
            }
        }

        #endregion

    }
}
