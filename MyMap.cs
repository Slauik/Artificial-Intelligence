using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Homm.Client
{
    // Класс MyMap - локальная запись всех открытых участков на карте
    class MyMap
    {
        // StreamWriter sw = new StreamWriter("map.txt");

        private HommSensorData sensorData;
        public int weight; // Длина карты
        public int height; // Глубина карты

        public Chain[,] cells; // Список ячеек на карте
        public List<TopItem> mines = new List<TopItem>(); // Список найденных шахт
        public List<TopItem> dwellings = new List<TopItem>(); // Список найденных таверн

        // Конструктор
        public MyMap(int weight, int height)
        {
            this.weight = weight;
            this.height = height;
            cells = new Chain[weight, height];
            InitMap();
        }

        // Метод-инициализатор, заполняет карту пустыми ячейками
        private void InitMap()
        {
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < weight; w++)
                {
                    cells[w, h] = new Chain(w, h);
                }
            }
        }

        public void Zaplatka()
        {
            if (sensorData.MyRespawnSide.Equals("Left"))
            {
                cells[weight-1, height-1].travel_cost = -1;
            }
            else
            {
                cells[0, 0].travel_cost = -1;
            }
        }

        // Метод, возвращающий текущую клетку из sensorData по посылаемым координатам
        public MapObjectData GetCurrentCell(int w, int h)
        {
            return sensorData.Map.Objects.
                          Where(cell => cell.Location.X == w && cell.Location.Y == h).
                          Select(cell => cell).FirstOrDefault();
        }

        // Метод заполянющий выбранную ячейку
        private void FillCurrentCell(int w, int h)
        {
            var temp = GetCurrentCell(w, h);

            // Если клетка находится в тумане войны
            if (temp == null)
            {
                return;
            }
            else
            {
                // Если стена, то пройти нельзя
                if (temp.Wall != null)
                {
                    cells[w, h].travel_cost = -1;
                }
                // Если не стена
                else
                {
                    // Записываем стоймость передвижения по клетке
                    cells[w, h].travel_cost = TileTerrain.Parse(temp.Terrain.ToString()[0]).TravelCost;

                    // Если на этой клетке находится таверна, добавляем ее в список таверн
                    if (temp.Dwelling != null)
                    {

                        if (w == 0 && h == 0 && sensorData.MyRespawnSide.Equals("Right") || w == weight - 1 && h == height - 1 && sensorData.MyRespawnSide.Equals("Left"))
                        { }
                        else
                            dwellings.Add(new TopItem(w, h, temp.Dwelling));
                    }
                    // Если на этой клетке находится шахта, добавляем ее в список шахт
                    if (temp.Mine != null)
                    {
                        mines.Add(new TopItem(w, h, temp.Mine));
                    }
                }
            }
        }

        // Метод обновления карты
        public void UpdateMap(HommSensorData data)
        {
            sensorData = data;
            for (int h = 0; h < height; h++)
            {
                for (int w = 0; w < weight; w++)
                {
                    // Если клетка пустая
                    if (cells[w, h].travel_cost == 0)
                    {
                        FillCurrentCell(w, h);
                    }
                }
            }

            UpdateDwelling();
            UpdateMine();
        }

        // Дописать для всех таверн, не только тех в которых мы наняли юнитов
        // Метод обновления таверн
        public void UpdateDwelling()
        {
            var fef = sensorData.Map.Objects.Where(o => o.Dwelling != null).Select(o => o);
            if (sensorData.Location.X == 3 && sensorData.Location.Y == 3)
            {

            }

            foreach (var item in fef)
            {
                int ind = dwellings.FindIndex(coord => coord == (TopItem)item.Location);
                if (ind != -1)
                {
                    dwellings[ind].dwellingIsHere = item.Dwelling;

                }
            }

            //// По списку
            //var index = dwellings.FindIndex(dw => dw.X == coord.X && dw.Y == coord.Y);
            //// По игровой карте
            //var dwel = GetCurrentCell(coord.X, coord.Y);
            //dwellings[index].dwellingIsHere = dwel.Dwelling;
        }

        // Дописать для всех таверн, не только тех в которых мы наняли юнитов
        // Метод обновления шахт
        public void UpdateMine()
        {
            var fef = sensorData.Map.Objects.Where(o => o.Mine != null).Select(o => o);

            foreach (var item in fef)
            {
                int ind = mines.FindIndex(coord => coord == (TopItem)item.Location);
                if (ind != -1)
                {
                    mines[ind].mineIsHere = item.Mine;
                }
            }

            //// По списку
            //var index = mines.FindIndex(dw => dw.X == coord.X && dw.Y == coord.Y);
            //// По игровой карте
            //var mns = GetCurrentCell(coord.X, coord.Y);
            //mines[index].mineIsHere = mns.Mine;
        }

        // Запись карты в файл
        //public void PrintMap()
        //{
        //    for (int h = 0; h < height; h++)
        //    {
        //        for (int w = 0; w < weight; w++)
        //        {
        //            sw.Write($"{cells[w, h].travel_cost, 6}|");
        //        }
        //        sw.WriteLine();
        //    }
        //    sw.Close();
        //}
    }
}
