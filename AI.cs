using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.IO;

namespace Homm.Client
{
    delegate void MAGIC(Place pl, List<TopItemPlaces> smthngs); // Делегат

    class AI
    {

        event MAGIC DOITPLS; // Событие

        private HommClient client; // Игровой клиент
        private HommSensorData sensorData; // Получаемые данные
        public Vision myVision; // Зрение

        private TopItemPlaces minePointer; // Указатель шахты
        private TopItemPlaces dwellingPointer; // Указатель таверны
        private TopItemPlaces resourcePointer; // Указатель ресурса

        AStarSolver pathSolver;

        // Конструктор
        public AI(HommSensorData sensorData, HommClient client)
        {
            this.client = client;
            this.sensorData = sensorData;
            myVision = new Vision(sensorData.Map);
            myVision.InitMap();
            minePointer.mineIsHere = new HoMM.ClientClasses.Mine(Resource.Gold, "kek");
            dwellingPointer.dwellingIsHere = new HoMM.ClientClasses.Dwelling(UnitType.Militia, 0, "kek");
            resourcePointer.resourceIsHere = new HoMM.ClientClasses.ResourcePile(Resource.Gold, 0);
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
            var dictionary = UnitsConstants.Current.UnitCost.Where(t => t.Key.Equals(unitType)).Select(t => t.Value).FirstOrDefault();

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

        #region SearchFor //Методы, для поиска шахты или таверны

        // Поиск всех объектов посылаемого типа
        private Place SearchFor(Place hero, TopItemPlaces type)
        {
            // Если ищем шахты
            if (type.mineIsHere != null)
            {
                var minesCoords = myVision.mines.Where(mine => !mine.mineIsHere.Owner.Equals(sensorData.MyRespawnSide)).Select(mine => mine.coord);

                return SearchNearest(hero, minesCoords);
            }
            // Иначе если ищем таверны
            else if (type.dwellingIsHere != null)
            {
                return SearchNearest(hero, myVision.dwellings.Select(min => min.coord));
            }
            // Иначе ищем ресурсы
            else
            {
                return SearchNearest(hero, myVision.resources.Select(min => min.coord));
            }
        }

        // Метод, который на известном участке карты будет искать ближающую точку с совпадающий передаваемым параметром
        private Place SearchFor(Place hero, Resource res)
        {
            // Ищем в листе mines все шахты ресурса посылаемого типа
            var mineList = myVision.mines.Where(min => min.mineIsHere.Resource.Equals(res)).Select(min => min.coord);
            // Возвращаем координаты ближайшей шахты
            return SearchNearest(hero, mineList);
        }

        // Перегрузка для поиска таверн
        // Метод, который на известном участке карты будет искать ближающую точку с совпадающий передаваемым параметром
        private Place SearchFor(Place hero, UnitType uni)
        {
            // Ищем в листе dwellings все таверны юнитов посылаемого типа
            var dwellingList = myVision.dwellings.Where(min => min.dwellingIsHere.UnitType.Equals(uni)).Select(min => min.coord);
            // Возвращаем координаты ближайшей таверны
            return SearchNearest(hero, dwellingList);
        }

        // Перегрузка ...
        //Метод, который ориентируясь, по перечислению ищет ближающую точку
        private Place SearchNearest(Place hero, IEnumerable<Place> searchingList)
        {
            List<Chain> timePath = new List<Chain>();

            foreach (var item in searchingList)
            {
                timePath.Add(pathSolver.GoTo(hero, item));
            }

            return timePath.
                Where(t => t.travel_time == timePath.Min(d => d.travel_time)).
                Select(dr => new Place(dr.X, dr.Y)).FirstOrDefault();
        }

        #endregion 

        // Метод запускающий работу ИИ
        internal void Start()
        {
            var temp = Program.MyProperty;
            temp.OnSensorDataReceived += myVision.Update;

            // Получаем наши начальные координаты
            Place myLocation = (Place)sensorData.Location;

            // Передаем в класс AStarSolver открытый участок карты, все известные нам шахты, таверны и ресусры
            pathSolver = new AStarSolver(myVision.bottom_map, myVision.mines, myVision.dwellings, myVision.resources, myVision.neutralarmies, sensorData.MyArmy);

            // Если поблизости нет шахт
            if (myVision.mines.Count == 0)
            {
                // Если поблизости нет ресурсов
                if (myVision.resources.Count == 0)
                {

                }
                // Если поблизости есть хотя бы одна кучка ресурсов
                else
                {

                }
            }
            // Если поблизости есть хотя бы одна шахта
            else
            {
                GoToMine(myLocation, myVision.mines);

                //// Ищем путь к ближайшей шахте
                //var pathToMine = AStarSolver.StringToDirection(pathSolver.GoTo(myLocation, SearchFor(myLocation, minePointer)).path);
                //// Список ресурсов по пути к шахте
                //var x = pathSolver.FindResourceByTheWay(myLocation, pathToMine);
                // Путь к ближайшему ресурсу
                //var pathToRes = AStarSolver.StringToDirection(pathSolver.GoTo(myLocation, SearchNearest(myLocation, x)).path);
                // Движение к ближайшему ресурсу (на 1 шаг)
                //sensorData = client.Move(pathToRes[0]);
            }

            //// Проверка
            //// Ищем ближайшую таверну типа пехоты (на карте отладки) и движемся к ней  
            //var path = AStarSolver.StringToDirection(pathSolver.GoTo((Place)sensorData.Location, SearchFor((Place)sensorData.Location, UnitType.Infantry)).path);
            //foreach (var e in path) sensorData = client.Move(e);


            //// Проверка
            //// В месте текущего положения героя проверяем может ли он купить из таверны юнитов
            //var unit = myVision.dwellings.
            //    Where(dw => dw.coord.Equals(myLocation)).
            //    Select(dw => dw.dwellingIsHere.UnitType).FirstOrDefault(); 

            //var amount = myVision.dwellings.
            //    Where(dw => dw.coord.Equals(myLocation)).
            //    Select(dw => dw.dwellingIsHere.AvailableToBuyCount).FirstOrDefault();

            //var buy = CanIHireUnit(unit, amount);
            //if (buy > 0)
            //{
            //    sensorData = client.HireUnits(buy);
            //}

        }

        // Движение к ближайшей доступной шахте 
        private void GoToMine(Place myLocation, List<TopItemPlaces> mines)
        {
            // Локальная копия всех известных шахт
            var localmines = mines;

            // Путь к ближайшей шахте
            var nearestMinePath = new Chain(-1, -1);

            // Пока не нашли маршрут к шахте
            while (nearestMinePath.X == -1 && nearestMinePath.Y == -1)
            {
                // Ищем ближайшую шахту
                var nearestMinePlace = SearchFor(myLocation, minePointer);
                // Определяем путь к ней
                nearestMinePath = pathSolver.GoTo(myLocation, nearestMinePlace);
                // Если путь не был найден (путь прегражадет какой-нибудь гарнизон, враг или нейтральная армия, 
                // которую нельзя одолеть на данный момент)
                if (nearestMinePath.X == -1 && nearestMinePath.Y == -1)
                {
                    // Удаляем эту шахту из локальной копии шахт
                    localmines.Remove(localmines.Where(p => p.coord == nearestMinePlace).Select(p => p).FirstOrDefault());
                }
                // Иначе если путь был найден, совершаем один шаг в этом направлении
                else
                {
                    // Получаем массив движений к ближайшей шахте
                    var dir_move = AStarSolver.StringToDirection(nearestMinePath.path);

                    // Находим список ресурсов лежащих рядом с каждой клеткой маршрута
                    var list_res = pathSolver.FindResourceByTheWay(myLocation, dir_move);

                    // Путь к ближайшему ресурсу
                    Direction[] pathToRes;

                    // Если был найден хотя бы один ресурс вблизи маршрута
                    if (list_res.Count != 0)
                    {
                        // то ищем к нему путь
                        pathToRes = AStarSolver.StringToDirection(pathSolver.GoTo(myLocation, SearchNearest(myLocation, list_res)).path);
                    }
                    // Если нет ресурсов поблизости
                    else
                    {
                        // Проверяем можем ли мы нанять юнитов на имеющиеся у нас ресурсы
                        var kek1 = sensorData.MyTreasury.Where(tr => tr.Value > 0)/*.OrderByDescending(tr => tr.Key)*/.Select(tr => tr).ToDictionary(tr => tr.Key, tr => tr.Value);

                        // Если у нас нет золота, то продолжаем продвижение к шахте
                        if (kek1.Where(ke => ke.Key.Equals(Resource.Gold)).Select(ke => ke.Value).FirstOrDefault() < 0)
                        {
                            // задаем нулевой путь
                            pathToRes = new Direction[0];
                        }
                        else
                        {
                            Console.WriteLine("ЗОЛОТЦЕ");

                        }

                        //foreach (var item in kek1)
                        //{
                        //    Console.WriteLine($"{item.Key.ToString()} + {item.Value}");
                        //}
                        Console.WriteLine("---------------------------");

                        // задаем нулевой путь
                        pathToRes = new Direction[0];
                    }



                    // Если нет ресурсов поблизости (на расстоянии одной клетки)
                    if (pathToRes.Length == 0)
                    {
                        // Двигаемся по направлению к шахте
                        sensorData = client.Move(dir_move[0]);
                    }
                    // Иначе если есть ресурс, двигаемся по направлению к ближайшему
                    else
                    {
                        sensorData = client.Move(pathToRes[0]);
                    }

                    if (myVision.mines.Where(mine => !mine.mineIsHere.Owner.Equals(sensorData.MyRespawnSide)).Select(mine => mine).Count() != 0)
                    {
                        // Рекурсивно вызываем метод, в который посылаем обновленные координаты ИИ и список шахт
                        GoToMine((Place)sensorData.Location, localmines);
                    }
                    // Другое действие
                    else
                    {

                    }
                        
                }
            }
        }

        // Движение к ближайшей доступной таверне 
        private void GoToDwelling(Place myLocation, List<TopItemPlaces> dwellings)
        {

        }

    }
}
