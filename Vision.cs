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

    // Структура Bottom - тип элементов карты недвижимых объектов
    struct Bottom
    {
        public int X; // Позиция по оси Х
        public int Y; // Позиция по оси Y
        public Place coord; // Поле Place, записывающее координаты X, Y
        public double travelCost; // Время передвижения по данной ячейке
        public MineEnter mineIsHere; // Есть ли на данной ячейке шахта 
        public DwellingEnter dwellingIsHere; // Есть ли на данной ячейке пункт для найма юнитов

        // Конструктор
        public Bottom(int X, int Y, double travelCost)
        {
            this.X = X;
            this.Y = Y;
            this.travelCost = travelCost;
            coord = new Place(X, Y);
            // ЗДАРОВО, КОСТЫЛЬ!
            // Нельзя назначать нулевую ссылку на поля, иначе не будте работать, сверхразум
            mineIsHere = new MineEnter(Resource.Ebony, "kek");
            //mineIsHere = null;
            dwellingIsHere = new DwellingEnter(UnitType.Cavalry, 0, "kek");
        }

        // Перегруженный конструктор
        public Bottom(Place coord, double travelCost) : this(coord.X, coord.Y, travelCost) { }

    }

    // Структура Top - тип элементов карты объектов, изменяющих свое состояние
    struct Top
    {
        public int X; // Позиция по оси Х
        public int Y; // Позиция по оси Y
        public Place coord; // Поле Place, записывающее координаты X, Y
        public Garrison garrisonIsHere; // Есть ли на данной ячейке гарнизон
        public NeutralArmy neutralArmyIsHere; // Есть ли на данной ячейке нейтральная армия
        public ResourcePile resourcePileIsHere; // Есть ли на данной ячейке куча с ресурсами
        public Hero heroIsHere; // Есть ли на данной ячейке вражеский герой

        // Конструктор
        public Top(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            coord = new Place(X, Y);
            // СУЙ сюда еще костыли
            garrisonIsHere = null;
            neutralArmyIsHere = null;
            resourcePileIsHere = null;
            heroIsHere = null;
        }

        // Перегруженный конструктор
        public Top(Place coord) : this(coord.X, coord.Y) { }
    }

    // Класс Vision - ИИ будет использовать зрение для ориентации в игровом мире
    class Vision
    {
        // Запись карты 
        StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory() + @"\map.txt");

        public Bottom[,] bottom_map; // Карта недвижимых объектов 
        //public Top[,] top_map; // Карта изменяющихся объектов
        public int widht, height; // Длина и ширина карты

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

        // Метод, заносящий послаемую клетку в карту Bottom
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
                if (temp.Mine != null /*.ToString().Contains("Mine")*/)
                {
                    // Записываем владельца этой шахты и тип добываемого ресурса
                    bottom_map[w, h].mineIsHere.Owner = temp.Mine.Owner.ToString(); /*(temp.Mine.Owner == null ? "empty" : temp.Mine.Owner.ToString());*/
                    bottom_map[w, h].mineIsHere.Resource = temp.Mine.Resource;
                }
                // Иначе если на данной клетке находится вход в пункт найма юнитов
                else if (temp.Dwelling != null /*.ToString().Contains("Dwelling")*/)
                {
                    // Записываем владельца этого пункта, тип юнитов нанимаемых здесь и допустимое количество для найма
                    bottom_map[w, h].dwellingIsHere.Owner = temp.Dwelling.Owner;
                    bottom_map[w, h].dwellingIsHere.UnitType = temp.Dwelling.UnitType;
                    bottom_map[w, h].dwellingIsHere.AvailableToBuyCount = temp.Dwelling.AvailableToBuyCount;
                }

            }
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
                        //FillCurrentTop(w, h);
                    }
                    sw.Write($"{bottom_map[w, h].travelCost,5}\t\t");
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
    }
}
