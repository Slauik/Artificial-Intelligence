using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.IO;

namespace Homm.Client
{
    // Структура Place - координаты
    public struct Place
    {
        // Поля-координаты
        public int X;
        public int Y;

        // Конструктор
        public Place(int ax, int ay)
        {
            X = ax;
            Y = ay;
        }

        public static explicit operator Place(LocationInfo v) //явное приведение типа
        {
            return new Place(v.X, v.Y);
        }
    }

    class Program
    {
        // Вставьте сюда свой личный CvarcTag для того, чтобы учавствовать в онлайн соревнованиях.
        public static readonly Guid CvarcTag = Guid.Parse("00000000-0000-0000-0000-000000000000");

        private static HommClient client;
        private static HommSensorData sensorData;

        public static void Main(string[] args)
        {
            Connect(args); //устанавливаем подключение к серверу


            AI ai = new AI(sensorData);
            AStarSolver pathSolver = new AStarSolver(/*sensorData.Map*/ai.myVision.bottom_map);
            var path = pathSolver.GoTo(sensorData.Location, new LocationInfo(1, 1));
            foreach (var e in path) sensorData = client.Move(e);

            path = pathSolver.GoTo(sensorData.Location, new LocationInfo(0, 0));
            foreach (var e in path) sensorData = client.Move(e);

            sensorData = client.HireUnits(1);

            // Перемещаемся по полученному пути

            client.Exit();
        }

        //Подключение к серверу
        private static void Connect(string[] args)
        {
            if (args.Length == 0)
                args = new[] { "127.0.0.1", "18700" };
            var ip = args[0];
            var port = int.Parse(args[1]);

            client = new HommClient();

            client.OnSensorDataReceived += Print;
            client.OnInfo += OnInfo;

            sensorData = client.Configurate(
                ip, port, CvarcTag,

                timeLimit: 1000,              // Продолжительность матча в секундах (исключая время, которое "думает" ваша программа). 

                operationalTimeLimit: 1000,   // Суммарное время в секундах, которое разрешается "думать" вашей программе. 
                                              // Вы можете увеличить это время для отладки, чтобы ваш клиент не был отключен, 
                                              // пока вы разглядываете программу в режиме дебаггинга.

                seed: 0,                    // Seed карты. Используйте этот параметр, чтобы получать одну и ту же карту и отлаживаться на ней.
                                            // Иногда меняйте этот параметр, потому что ваш код должен хорошо работать на любой карте.

                spectacularView: true,      // Вы можете отключить графон, заменив параметр на false.

                debugMap: false,            // Вы можете использовать отладочную простую карту, чтобы лучше понять, как устроен игоровой мир.

                level: HommLevel.Level2,    // Здесь можно выбрать уровень. На уровне два на карте присутствует оппонент.

                isOnLeftSide: true          // Вы можете указать, с какой стороны будет находиться замок героя при игре на втором уровне.
                                            // Помните, что на сервере выбор стороны осуществляется случайным образом, поэтому ваш код
                                            // должен работать одинаково хорошо в обоих случаях.
            );
        }
        //Вывод информации о соседних с персонажем ячейках
        static void Print(HommSensorData data)
        {
            //infoSW.WriteLine($"{step}");
            //infoSW.WriteLine($"Герой мертв? {data.IsDead.ToString() }");
            //infoSW.WriteLine($"Положение героя: {data.Location.ToString()}");
            //GetFuckingMap(data.Map);
            //infoSW.WriteLine($"Моя армия: {data.MyArmy.Select(z => z.Value + " " + z.Key).Aggregate((a, b) => a + ", " + b)}");
            //infoSW.WriteLine($"Сторона возрождения: {data.MyRespawnSide.ToString() }");
            //infoSW.WriteLine($"мои ресурсы: {data.MyTreasury.Select(z => z.Value + " " + z.Key).Aggregate((a, b) => a + ", " + b)}");
            //infoSW.WriteLine("---------------------------------");

            //mapSW.WriteLine($"{step}");
            //GetMap(data.Map);


            //step++;

            Console.WriteLine("---------------------------------");

            Console.WriteLine($"You are here: ({data.Location.X},{data.Location.Y})");

            Console.WriteLine($"You have {data.MyTreasury.Select(z => z.Value + " " + z.Key).Aggregate((a, b) => a + ", " + b)}");

            var location = data.Location.ToLocation();

            Console.Write("W: ");
            Console.WriteLine(GetObjectAt(data.Map, location.NeighborAt(Direction.Up)));

            Console.Write("E: ");
            Console.WriteLine(GetObjectAt(data.Map, location.NeighborAt(Direction.RightUp)));

            Console.Write("D: ");
            Console.WriteLine(GetObjectAt(data.Map, location.NeighborAt(Direction.RightDown)));

            Console.Write("S: ");
            Console.WriteLine(GetObjectAt(data.Map, location.NeighborAt(Direction.Down)));

            Console.Write("A: ");
            Console.WriteLine(GetObjectAt(data.Map, location.NeighborAt(Direction.LeftDown)));

            Console.Write("Q: ");
            Console.WriteLine(GetObjectAt(data.Map, location.NeighborAt(Direction.LeftUp)));
            //Console.ReadLine();
        }

        //Получить информацию о ячейке
        static string GetObjectAt(MapData map, Location location)
        {
            if (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height)
                return "Outside";

            //string temp = "";
            return map.Objects.
                Where(x => x.Location.X == location.X && x.Location.Y == location.Y)
                .FirstOrDefault()?.ToString() ?? "Nothing";
        }

        static void OnInfo(string infoMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(infoMessage);
            Console.ResetColor();
        }
       
    }
}