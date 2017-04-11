using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.Threading;

namespace Homm.Client
{

    class Program
    {
        // Вставьте сюда свой личный CvarcTag для того, чтобы учавствовать в онлайн соревнованиях.
        //Димин  400f8d33-14ac-48d0-aafc-391b819038e4
        //Славин a9da6916-83fd-470e-838c-d8b4144ba434
        public static readonly Guid CvarcTag = Guid.Parse("a9da6916-83fd-470e-838c-d8b4144ba434");

        private static HommSensorData sensorData;
        private static HommClient client;

        public static void Main(string[] args)
        {
            Connect(args);
            BestAI ai = new BestAI(sensorData, client);

            client.OnSensorDataReceived += Print;
            // Сообщаем с событием OnSensorDataReceived метод обновления локальной карты
            // списков шахт и ресурсов
            client.OnSensorDataReceived += ai.Updates;
            client.OnInfo += OnInfo;
            ai.ThinkingWhatToDoNext();
            client.Exit();
        }

        private static void Connect(string[] args)
        {
            if (args.Length == 0)
                //args = new[] { "homm.ulearn.me", "18700" };
            args = new[] { "127.0.0.1", "18700" };
            var ip = args[0];
            var port = int.Parse(args[1]);

            client = new HommClient();

            sensorData = client.Configurate(
              ip, port, CvarcTag,

              timeLimit: 500,              // Продолжительность матча в секундах (исключая время, которое "думает" ваша программа). 

              operationalTimeLimit: 500,   // Суммарное время в секундах, которое разрешается "думать" вашей программе. 
                                          // Вы можете увеличить это время для отладки, чтобы ваш клиент не был отключен, 
                                          // пока вы разглядываете программу в режиме дебаггинга.

              seed: 9,                    // Seed карты. Используйте этот параметр, чтобы получать одну и ту же карту и отлаживаться на ней.
                                          // Иногда меняйте этот параметр, потому что ваш код должен хорошо работать на любой карте.

              spectacularView: true,      // Вы можете отключить графон, заменив параметр на false.

              debugMap: false,            // Вы можете использовать отладочную простую карту, чтобы лучше понять, как устроен игоровой мир.

              level: HommLevel.Level3,    // Здесь можно выбрать уровень. На уровне два на карте присутствует оппонент.

              isOnLeftSide: false          // Вы можете указать, с какой стороны будет находиться замок героя при игре на втором уровне.
                                          // Помните, что на сервере выбор стороны осуществляется случайным образом, поэтому ваш код
                                          // должен работать одинаково хорошо в обоих случаях.
          );

        }

        static void Print(HommSensorData data)
        {
            Console.WriteLine("---------------------------------");

            Console.WriteLine($"You are here: ({data.Location.X},{data.Location.Y})");

            Console.WriteLine($"You have {data.MyTreasury.Select(z => z.Value + " " + z.Key).Aggregate((a, b) => a + ", " + b)}");

            Console.WriteLine($"My army: {data.MyArmy.Select(dct => dct.Key + " " + dct.Value).Aggregate((a, b) => a + ", " + b)}");

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
        }

        static string GetObjectAt(MapData map, Location location)
        {
            if (location.X < 0 || location.X >= map.Width || location.Y < 0 || location.Y >= map.Height)
                return "Outside";
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