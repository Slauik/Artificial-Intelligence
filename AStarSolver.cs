using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.Diagnostics;

// - Изменить метод InRange для вражеских гарнизонов

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
        public static explicit operator Place(Chain v)
        {
            return new Place(v.X, v.Y);
        }

        public bool Equals(Place pl)
        {
            return X == pl.X && Y == pl.Y;
        }
        public static bool operator ==(Place left, Place right)
        {
            return Equals(left, right);
        }
        public static bool operator !=(Place left, Place right)
        {
            return !Equals(left, right);
        }

        public static Place operator +(Place current, Place shift)
        {
            //В зависимости от координаты Х шестигранника current меняется алгоритм перемещения по диагоналям
            //Если мы совершаем движение со сдвигом по координате Х (в сторону)
            if (shift.X != 0) // X==1 || X==-1
            {
                //в зависимости от текущего положения
                //не изменяем координату Y, если Х был чётный, а движение совершалось в сторону и вниз
                //или если X был не чётный, а движение совершалось в сторону и вверх
                if (current.X % 2 == 0 && shift.Y == 1 || current.X % 2 == 1 && shift.Y == -1)
                {
                    return new Place(current.X + shift.X, current.Y);
                }
            }
            // возвращаем координатную сумму
            return new Place(current.X + shift.X, current.Y + shift.Y);
        }



        public static explicit operator Place(Direction point)
        {
            switch (point)
            {
                case Direction.Up:
                    return new Place(0, -1);
                case Direction.Down:
                    return new Place(0, 1);
                case Direction.LeftUp:
                    return new Place(-1, -1);
                case Direction.LeftDown:
                    return new Place(-1, 1);
                case Direction.RightUp:
                    return new Place(1, -1);
                case Direction.RightDown:
                    return new Place(1, 1);
                default:
                    break;
            }
            return new Place(0, 0);
        }

    }

    // Структура звена
    public struct Chain
    {
        // Поля координат
        public int X;
        public int Y;
        public string path; // Поле, запоминающее путь перемещения объекта
        public double travel_time; // Поле, считающее время пути path

        // Конструктор
        public Chain(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
            this.path = "";
            this.travel_time = Double.MaxValue;
        }
        public Chain(Place place) : this(place.X, place.Y) { } //Перегруженный конструктор

        // Конструктор, заполняющий время и путь
        public Chain(Place place, string path, double travel_time) : this(place)
        {
            this.path = path;
            this.travel_time = travel_time;
        }

        // Конструктор, заполняющий путь
        public Chain(int X, int Y, string path) : this(X, Y)
        {
            this.path = path;
        }
        public Chain(Place place, string path) : this(place.X, place.Y, path) { } // Перегруженный Конструктор, заполняющий путь

        // конструктор, заполняющий время
        public Chain(int X, int Y, double travel_time) : this(X, Y)
        {
            this.travel_time = travel_time;
        }
        public Chain(Place place, double travel_time) : this(place.X, place.Y, travel_time) { } // Перегруженный Конструктор, заполняющий время

        // Переопределение оператора "+"
        public static Place operator +(Chain current, Chain shift)
        {
            //В зависимости от координаты Х шестигранника current меняется алгоритм перемещения по диагоналям
            //Если мы совершаем движение со сдвигом по координате Х (в сторону)
            if (shift.X != 0) // X==1 || X==-1
            {
                //в зависимости от текущего положения
                //не изменяем координату Y, если Х был чётный, а движение совершалось в сторону и вниз
                //или если X был не чётный, а движение совершалось в сторону и вверх
                if (current.X % 2 == 0 && shift.Y == 1 || current.X % 2 == 1 && shift.Y == -1)
                {
                    return new Place(current.X + shift.X, current.Y);
                }
            }
            // возвращаем координатную сумму
            return new Place(current.X + shift.X, current.Y + shift.Y);
        }
    }

    class AStarSolver
    {
        // Список звеньев передвижения
        private List<Chain> directions;
        // Карта недвижимых частей игрового мира
        private Bottom[,] bottom_map;
        // Списки изменяемых объектов
        private List<TopItemPlaces> mines;
        private List<TopItemPlaces> dwellings;
        private List<TopItemPlaces> resources;
        private List<TopItemPlaces> neutralarmies;
        private Dictionary<UnitType, int> myarmy;

        // конструктор, инициализирующий карту и заполняющий список звеньев передвижения
        public AStarSolver(Bottom[,] bottom_map, List<TopItemPlaces> mines, List<TopItemPlaces> dwellings, List<TopItemPlaces> resources, List<TopItemPlaces> neutralarmies, Dictionary<UnitType, int> myarmy)
        {
            this.bottom_map = bottom_map;
            this.mines = mines;
            this.dwellings = dwellings;
            this.resources = resources;
            this.neutralarmies = neutralarmies;
            this.myarmy = myarmy;
            directions = new List<Chain>();
            directions.Add(new Chain(-1, -1, ((int)Direction.LeftUp).ToString())); // движение вверх-влево
            directions.Add(new Chain(-1, 1, ((int)Direction.LeftDown).ToString())); // движение вниз-влево
            directions.Add(new Chain(1, -1, ((int)Direction.RightUp).ToString())); // движение вверх-вправо
            directions.Add(new Chain(1, 1, ((int)Direction.RightDown).ToString())); // движение вниз-вправо
            directions.Add(new Chain(0, -1, ((int)Direction.Up).ToString())); // движение вверх
            directions.Add(new Chain(0, 1, ((int)Direction.Down).ToString())); // движение вниз
        }

        // Метод GoTO возвращающий массив передвижений для 
        // перемещения из координты start в координату finish 
        public Chain GoTo(Place start, Place finish)
        {
            // получаем оптимальный путь перемещеня из точки start в finish в виде строки
            return ASolverMove(start, finish);
        }

        // Ищем ресурсы по дороге, не включая ресурсы уже лежащие на нашем пути
        public List<Place> FindResourceByTheWay(Place hero, Direction[] path)
        {
            // Список, который забирает ресурсы по пути
            List<Place> resNearRoad = new List<Place>();

            // Список ресурсов, которые собирает ИИ проходя клетку
            List<Place> resOnRoad = new List<Place>();

            // Смотрим на ресурсы вокруг нашего ИИ
            var temp = hero;

            // Цикл по пути, начинаем не с первого перемещения
            for (int i = 0; i < path.Length; i++)
            {
                // Если на текущей точке находится ресурс, то добавляем его в список resOnRoad, 
                // т.к. мы автоматически его забираем
                if (resNearRoad.Contains(temp))
                {
                    resOnRoad.Add(temp);
                }

                // Проверяем все возможные направления вокруг текущей ячейки
                foreach (var item in directions)
                {
                    // Проверяем ячейку со сдвигом в сторону
                    var testing_cell = temp + (Place)item;
                    // Мы уже проверяли эту ячейку?
                    if (resNearRoad.Contains(testing_cell))
                    {
                        continue;
                    }
                    
                    // Ищем в списке ресурсов ресурс расположенный на проверяемой ячейке
                    var x = resources.Where(c => c.coord == testing_cell).FirstOrDefault();

                    // Если на этой позиции нет ресурса, то рассматриваем другую координату
                    if (x.resourceIsHere == null)
                    {
                        continue;
                    }

                    // Иначе добавляем этот ресурс в список ресурсов
                    resNearRoad.Add(testing_cell);
                }

                // Совершаем "переход" на следующую координату
                temp += (Place)path[i];
            }
            // Если не было найдено ближайших ресурсов, возвращаем пустой список
            if (resNearRoad.Count == 0)
            {
                return new List<Place>();
            }
            // Возвращаем ресурсы, находящиеся рядом с путем
            return resNearRoad.Except(resOnRoad).ToList();
        }

        // Метод ASolverMove находит оптимальный маршрут перемещения из точки start 
        // в точку finish
        private Chain ASolverMove(Place start, Place finish)
        {
            // Создаем очередь звеньев непосещенных ячеек
            Queue<Chain> queue = new Queue<Chain>();
            // Создаем список посещенных ячеек
            List<Place> visited_list = new List<Place>();

            // Очищаем очередь и спиоск
            queue.Clear();
            visited_list.Clear();

            // Создаем начальное звено
            Chain chain = new Chain(start, 0);
            Place place;

            // Добавляем звено в очередь на проверку
            queue.Enqueue(chain);

            while (queue.Count > 0)
            {
                // Извлекаем находящееся первое звено в очереди
                chain = queue.Dequeue();

                // Проверяем на возможность перехода из этого звена в соседние ячейки
                foreach (Chain side in directions)
                {
                    // задаем соседнюю ячейку
                    place = chain + side;

                    if (!InRange(place)) { continue; } //проверка на возможность пройти
                    if (visited_list.Contains(place)) { continue; } //проверка на список проверенных мест

                    // Проверка на нейтральные армии
                    if (InDanger(place))
                    {
                        visited_list.Add(place);
                        continue;
                    }

                    // если были пройдены все проверки до этого, добавляем 
                    // соседнюю ячейку в список проверенных мест
                    visited_list.Add(place);

                    // увеличение пути и времени
                    Chain step = new Chain(place, chain.path + side.path);
                    step.travel_time = chain.travel_time + bottom_map[place.X, place.Y].travelCost;

                    if (place.Equals(finish) /*&& step.travel_time < true_path.travel_time*/) //проверка на достижение цели
                    {
                        return step;
                    }
                    // добавляем в очередь соседнюю ячейку, если она не оказалась финишом
                    queue.Enqueue(step);
                }
            }
            // Если путь из точки start в точку finish не был найден, возвращаем
            return new Chain(-1, -1);
        }

        // Если на пути враг, которого победить нельзя, возвращаем true
        private bool InDanger(Place place)
        {
            var enemy = neutralarmies.Where(x => x.coord == place).Select(x => x.neutralArmyIsHere).FirstOrDefault();//.Select(x => x.neutralArmyIsHere.Army).FirstOrDefault();

            // Если на пути есть враг
            if (enemy != null)
            {
                // И мы не можем его победить, то возвращаем false
                if ((Combat.Resolve(new ArmiesPair(myarmy, enemy.Army))).IsDefenderWin)
                {
                    return true;
                }

            }
            return false;
        }

        // Метод проверяющий возможность перехода в переданную ячейку
        private bool InRange(Place place)
        {
            // Если мы оказываемся за пределами игрового мира, возвращаем false
            if (place.X < 0 || place.X >= bottom_map.GetLength(0) || place.Y < 0 || place.Y >= bottom_map.GetLength(1))
                return false;

            if (bottom_map[place.X, place.Y].travelCost == -1)
            {
                return false;
            }

            // Если все проверки пройдены, вернуть true
            return true;
        }

        public static Direction[] StringToDirection(string path)
        {
            // Преобразуем полученную строку в массив движений на игровом поле
            Direction[] dir_path = new Direction[path.Length];

            for (int i = 0; i < dir_path.Length; i++)
            {
                switch (path[i])
                {
                    case '0':
                        dir_path[i] = Direction.Up;
                        break;
                    case '1':
                        dir_path[i] = Direction.Down;
                        break;
                    case '2':
                        dir_path[i] = Direction.LeftUp;
                        break;
                    case '3':
                        dir_path[i] = Direction.LeftDown;
                        break;
                    case '4':
                        dir_path[i] = Direction.RightUp;
                        break;
                    case '5':
                        dir_path[i] = Direction.RightDown;
                        break;
                    default:
                        break;
                }
            }
            return dir_path;
        }

    }
}
