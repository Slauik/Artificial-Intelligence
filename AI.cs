using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.IO;

namespace Homm.Client
{
    class AI
    {
        private HommClient client; // Игровой клиент
        private HommSensorData sensorData; // Получаемые данные
        public Vision myVision; // Зрение

        // Конструктор
        public AI(HommSensorData sensorData, HommClient client)
        {
            this.client = client;
            this.sensorData = sensorData;
            myVision = new Vision(sensorData.Map);
            myVision.InitMap();
        }

        // Метод возвращающий количество ресурса посылаемого типа имеющегося у ИИ
        private int myResourse(Resource resType)
        {
            return sensorData.MyTreasury.Where(t => t.Key.Equals(resType)).Select(t => t.Value).FirstOrDefault();
        }

        // Метод проверяющий можно ли нанять данный тип юнитов
        private int CanIHireUnit(UnitType unitType, int avaliableCount) 
        {
            // Если у нас нет денег или здесь нет юнитов возвращаем false
            if (myResourse(Resource.Gold) <= 0 || avaliableCount <= 0) 
            {
                return 0;
            }

            // Иначе получаем словарь стоймости юнитов данного типа
            var dictionary = UnitsConstants.Current.UnitCost.Where(t=>t.Key.Equals(unitType)).Select(t=>t.Value).FirstOrDefault();

            // Задаем минимальное значение юнитов, которое мы можем приобрести
            int min = avaliableCount;

            // Проверяем наличие ресурсов
            foreach (var item in dictionary)
            {
                // Если у нас есть ресусры данного типа на покупку хотя бы одного юнита 
                if (myResourse(item.Key) >= item.Value)
                {
                    // Смотрим сколько юнитов можем купить имея только данный ресурс
                    // и сравниваем с min
                    if (min > myResourse(item.Key) / item.Value)
                    {
                        min = myResourse(item.Key) / item.Value;
                    }
                    
                    continue;
                }
                // Иначе если у нас нет какого либо ресурса, возвращаем 0
                else
                {
                    return 0;
                }
            }
            return min;
        }

        // Метод запускающий работу ИИ
        internal void Start()
        {

            //while (true)
            //{

            //}
            Place myLocation = (Place)sensorData.Location;

            AStarSolver pathSolver = new AStarSolver(myVision.bottom_map);

            var path = pathSolver.GoTo(sensorData.Location, new LocationInfo(1, 1));
            foreach (var e in path) sensorData = client.Move(e);

            path = pathSolver.GoTo(sensorData.Location, new LocationInfo(0, 0));
            foreach (var e in path) sensorData = client.Move(e);


            // Исправленно, но костыль :D
            var unit = myVision.bottom_map[myLocation.X, myLocation.Y].dwellingIsHere.UnitType;
            var amount = myVision.bottom_map[myLocation.X, myLocation.Y].dwellingIsHere.AvailableToBuyCount;
            var buy = CanIHireUnit(unit, amount);
            sensorData = client.HireUnits(buy);

            //sensorData = client.HireUnits(CanIHireUnit(myVision.bottom_map[myLocation.X, myLocation.Y].
            //        dwellingIsHere.UnitType, myVision.bottom_map[myLocation.X, myLocation.Y].
            //        dwellingIsHere.AvailableToBuyCount));

        }

        public void Method()
        {
            Place myLocation = (Place)sensorData.Location;

            if (myVision.bottom_map[myLocation.X, myLocation.Y].dwellingIsHere != null)
            {
                if(CanIHireUnit(myVision.bottom_map[myLocation.X, myLocation.Y].
                    dwellingIsHere.UnitType, myVision.bottom_map[myLocation.X, myLocation.Y].
                    dwellingIsHere.AvailableToBuyCount) > 0)
                {
                    
                }
            }



            // Получаем путь из начальной точки в точку c координатмаи (0, 9)
            


            //var path = new[] { Direction.RightDown, Direction.RightUp, Direction.RightDown, Direction.RightUp, Direction.LeftDown, Direction.Down, Direction.RightDown, Direction.RightDown, Direction.RightUp };
            

        }
    }
}
