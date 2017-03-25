using System;
using System.Linq;
using HoMM.Sensors;
using HoMM;
using HoMM.ClientClasses;
using System.Collections.Generic;
using System.Diagnostics;

// - Изменить метод InRange для вражеских гарнизонов
// - Заменить Place на Chain

namespace Homm.Client
{
    class AStarSolver
    {
        // Список звеньев передвижения
        private List<Chain> directions;
        // Карта игрового мира
        private Bottom[,] map;
        // конструктор, инициализирующий карту и заполняющий список звеньев передвижения
        public AStarSolver(Bottom[,] map)
        {
            this.map = map;
            directions = new List<Chain>();
            directions.Add(new Chain(-1, -1, ((int)Direction.LeftUp).ToString())); // движение вверх-влево
            directions.Add(new Chain(-1,  1, ((int)Direction.LeftDown).ToString())); // движение вниз-влево
            directions.Add(new Chain( 1, -1, ((int)Direction.RightUp).ToString())); // движение вверх-вправо
            directions.Add(new Chain( 1,  1, ((int)Direction.RightDown).ToString())); // движение вниз-вправо
            directions.Add(new Chain( 0, -1, ((int)Direction.Up).ToString())); // движение вверх
            directions.Add(new Chain( 0,  1, ((int)Direction.Down).ToString())); // движение вниз
        }

        // Структура звена
        struct Chain
        {
            // Поля координат
            public int X;
            public int Y;
            public string path; // Поле, запоминающее путь перемещения объекта
            public double travel_time; // Поле, считающее время пути path

            // Конструктор
            public Chain(int x, int y)
            {
                this.X = x;
                this.Y = y;
                this.path = "";
                this.travel_time = Double.MaxValue;
            }
            //Перегруженный конструктор
            public Chain(Place place) : this(place.X, place.Y) { }

            // Конструктор, заполняющий путь
            public Chain(int x, int y, string path) : this(x, y)
            {
                this.path = path;
            }

            // Перегруженный конструктор
            public Chain(Place place, string path) : this(place.X, place.Y, path) { }

            // Конструктор, заполняющий время
            public Chain(int x, int y, string path, double travel_time) : this(x, y, path)
            {
                this.travel_time = travel_time;
            }

            // Перегруженный конструктор, заполняющий время
            public Chain(Place place, string path, double travel_time) : this(place.X, place.Y, path, travel_time) { }

            // Переопределение оператора "+"
            public static Place operator +(Chain current, Chain shift)
            {
                //В зависимости от координаты Х шестигранника current меняется алгоритм перемещения по диагоналям
                //Если мы совершаем движение со сдвигом по координате Х (в сторону)
                if (shift.X !=0) // X==1 || X==-1
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

        // Метод GoTO возвращающий массив передвижений для 
        // перемещения из координты start в координату finish 
        public Direction[] GoTo(LocationInfo start, LocationInfo finish)
        {
            // Создаем две переменные структуры Place 
            // с известными координатами начала и конца пути
            Place p_start = new Place(start.X, start.Y);
            Place p_finish = new Place(finish.X, finish.Y);

            // получаем оптимальный путь перемещеня из точки start в finish в виде строки
            string path = ASolverMove(p_start, p_finish);

            return StringToDirection(path);
        }

        // Метод ASolverMove находит оптимальный маршрут перемещения из точки start 
        // в точку finish
        private string ASolverMove(Place start, Place finish)
        {
            // Создаем очередь звеньев непосещенных ячеек
            Queue<Chain> queue = new Queue<Chain>();
            // Создаем список посещенных ячеек
            List<Place> visited_list = new List<Place>();

            // Очищаем очередь и спиоск
            queue.Clear();
            visited_list.Clear();

            // Создаем начальное звено
            Chain chain = new Chain(start);
            //Объект звена для выдачи окончательного пути
            Chain true_path = new Chain(start);

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

                    // если были пройдены все проверки до этого, добавляем 
                    // соседнюю ячейку в список проверенных мест
                    visited_list.Add(place);

                    // увеличение пути
                    Chain step = new Chain(place, chain.path + side.path);
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ВЕРОЯТНО ОШИБКА ЗДЕСЬ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                    step.travel_time = chain.travel_time + map[place.X, place.Y].travelCost;

                    //step.travel_time = chain.travel_time + TileTerrain.Parse(map.Objects. // Из списка объектов на карте
                    //Where(x => x.Location.X == place.X && x.Location.Y == place.Y). // находим текущую точку
                    //Select(x => x.Terrain). // выбираем тип ландшафта
                    //FirstOrDefault(). // приводим к типу Terrain
                    //ToString()[0]).TravelCost; // берем первый символ из слова ("Road" -> 'R') и получаем коэффициент

                    if (place.Equals(finish) && step.travel_time< true_path.travel_time) //проверка на достижение цели
                    {
//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!СЮДА НЕ ПОПАДАЕМ!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        true_path.travel_time = step.travel_time;
                        true_path.path = step.path;
                    }
                    // добавляем в очередь соседнюю ячейку, если она не оказалась финишом
                    queue.Enqueue(step);
                }
            }
            // Если путь из точки start в точку finish не был найден, возвращаем
            if (true_path.travel_time != Double.MaxValue) //проверка на достижение цели
            {
                return true_path.path.ToString();
            }
            return "";
        }

        // Метод проверяющий возможность перехода в переданную ячейку
        private bool InRange(Place place)
        {
            // Если мы оказываемся за пределами игрового мира, возвращаем false
            if (place.X < 0 || place.X >=  map.GetLength(0) || place.Y < 0 || place.Y >= map.GetLength(1))
                return false;

            // Считываем препятствие, находящееся на этой клетке
            //string barrier = map.Objects.
            //    Where(x => x.Location.X == place.X && x.Location.Y == place.Y)
            //    .FirstOrDefault().ToString();

            // Если это стена или гарнизон, возвращаем false
            //if (barrier.Equals("Wall") || barrier.StartsWith("Neutral army"))
            if(map[place.X,place.Y].travelCost==-1)
            {
                return false;
            }
            // Если все проверки пройдены, вернуть true
            return true;
        }

        private Direction[] StringToDirection(string path)
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
