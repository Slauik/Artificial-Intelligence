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
    }
    //Структура Bottom - тип элементов карты недвижимых эелементов
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

    class Program
    {
        //static FileStream info;
        //static FileStream _map;

        //static StreamWriter infoSW;
        //static StreamWriter mapSW;

        //static int step = 0;

        // Вставьте сюда свой личный CvarcTag для того, чтобы учавствовать в онлайн соревнованиях.
        public static readonly Guid CvarcTag = Guid.Parse("00000000-0000-0000-0000-000000000000");

        private static HommClient client;
        private static HommSensorData sensorData;

        public static void Main(string[] args)
        {
            //if (File.Exists(@"C:\Users\Слава\Desktop\info.txt"))
            //{
            //    File.Delete(@"C:\Users\Слава\Desktop\info.txt");
            //    File.Delete(@"C:\Users\Слава\Desktop\map.txt");
            //}
            //info = new FileStream(@"C:\Users\Слава\Desktop\info.txt", FileMode.OpenOrCreate, FileAccess.Write);
            //_map = new FileStream(@"C:\Users\Слава\Desktop\map.txt", FileMode.OpenOrCreate, FileAccess.Write);

            //infoSW = new StreamWriter(info);
            //mapSW = new StreamWriter(_map);

            Connect(args); //устанавливаем подключение к серверу

            sensorData = client.HireUnits(1);

            Vision v = new Vision(sensorData.Map);
            v.InitBottom();

            // Получаем путь из начальной точки в точку c координатмаи (0, 9)
            AStarSolver pathSolver = new AStarSolver(/*sensorData.Map*/v.bottom_map);
            var path = pathSolver.GoTo(sensorData.Location, new LocationInfo(4, 4));


            //var path = new[] { Direction.RightDown, Direction.RightUp, Direction.RightDown, Direction.RightUp, Direction.LeftDown, Direction.Down, Direction.RightDown, Direction.RightDown, Direction.RightUp };
            // Перемещаемся по полученному пути
            foreach (var e in path)
                sensorData = client.Move(e);


            //mapSW.Close();
            //infoSW.Close();


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

            var sensorData = client.Configurate(
                ip, port, CvarcTag,

                timeLimit: 1000,              // Продолжительность матча в секундах (исключая время, которое "думает" ваша программа). 

                operationalTimeLimit: 1000,   // Суммарное время в секундах, которое разрешается "думать" вашей программе. 
                                              // Вы можете увеличить это время для отладки, чтобы ваш клиент не был отключен, 
                                              // пока вы разглядываете программу в режиме дебаггинга.

                seed: 0,                    // Seed карты. Используйте этот параметр, чтобы получать одну и ту же карту и отлаживаться на ней.
                                            // Иногда меняйте этот параметр, потому что ваш код должен хорошо работать на любой карте.

                spectacularView: true,      // Вы можете отключить графон, заменив параметр на false.

                debugMap: false,            // Вы можете использовать отладочную простую карту, чтобы лучше понять, как устроен игоровой мир.

                level: HommLevel.Level3,    // Здесь можно выбрать уровень. На уровне два на карте присутствует оппонент.

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

        //static string GetMap(MapData map)
        //{
        //    int height = map.Height;
        //    int width = map.Width;

        //    for (int i = 0; i < height; i++)
        //    {
        //        for (int j = 0; j < width; j++)
        //        {
        //            mapSW.Write((GetObjectAt(map, new Location(i, j)))[0] + " ");
        //        }
        //        mapSW.WriteLine();
        //    }

        //    return " ";

        //}

        //static string GetFuckingMap(MapData map)
        //{
        //    int height = map.Height;
        //    int width = map.Width;

        //    for (int i = 0; i < height; i++)
        //    {
        //        for (int j = 0; j < width; j++)
        //        {
        //            infoSW.WriteLine($"({i}, {j})");

        //            var mas_tile = map.Objects.Where(x => x.Location.X == i && x.Location.Y == j).Select(x => x.Terrain);

        //            foreach (var item in mas_tile)
        //            {
        //                infoSW.WriteLine(item);
        //            }
        //            infoSW.WriteLine("------------------------------------------------------");
        //        }
        //    }
        //    return " ";
        //}


        //Получить информацию о ячейке
        static string GetObjectAt(MapData map, Location location)
        {
            if (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height)
                return "Outside";

            //string temp = "";
            return map.Objects.
                Where(x => x.Location.X == location.X && x.Location.Y == location.Y)
                .FirstOrDefault()?.ToString() ?? "Nothing";

            //if (temp.StartsWith("Hero"))
            //{
            //    return "H";
                
            //}
            //if (temp.StartsWith("Wall"))
            //{
            //    return "w";
            //}
            //if (temp.StartsWith("Garrison with"))
            //{
            //    return "g";
            //}
            //if (temp.StartsWith("Neutral army with"))
            //{
            //    return "n";
            //}
            //if (temp.StartsWith("Mine of"))
            //{
            //    return "m";
            //}
            //if (temp.StartsWith("Dwelling of"))
            //{
            //    return "d";
            //}
            //if (temp.StartsWith("Resource pile of"))
            //{
            //    return "r";
            //}
            //return "-";
        }

        static void OnInfo(string infoMessage)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(infoMessage);
            Console.ResetColor();
        }
       
    }
}