Artifical Intelligence v 1.1.0.1 (31.03.2017)
- Добавлен файл с блок-схемами
- Дописаны комментарии
	В файле AI.cs:
- В методе SearchFor осуществленна проверка на поиск не наших шахт
- Начало работы над обработчиком события
	В файле AStarSolver.cs:
- Поиск пути не учитывает время пути

Artifical Intelligence v 1.1.0.0 (31.03.2017)
- Добавлены комментарии
- Найдена ошибка в методе ASolverMove класса AStarSolver (см.комментарии)
	В файле Vision.cs:
- Добавлены инициализаторы полей в конструкторах структур типа TopItemPlaces
	В файле AStarSolver.cs:
- Исправлена ошибка в методе FindResourceByTheWay: теперь мы рассматриваем начальное положение героя

Artifical Intelligence v 1.0.1.5 (31.03.2017)
- Добавлены комментарии
- Неисправлена ошибка с методом Update класса Vision
	В файле Vision.cs:
- изменена структура TopItemPlaces
- В классе Vision добалено новое поле neutralarmies типа List<TopItemPlaces> 
- Модернизирован метод FillCurrentBottom 
- Добавлен метод Update
- Изменен метод UpdateResource
	В файле Homm.Client.Program.cs:
- Сообщаем метод Update из класса Vision с событием OnSensorDataReceived
	В файле AStarSolver.cs:
- В классе AStarSolver добавлены два поля: neutralarmies типа List<TopItemPlaces>, myarmy типа Dictionary<UnitType, int>
- Улучшен метод FindResourceByTheWay
- Добавлен метод InDanger, проверяющий нейтральные армии на пути ИИ
	В файле AI.cs:
- В классе AI добавлены новые поля
- Изменен метод SearchFor
- Добавлены методы SearchNearest
- Добавлен метод GoToMine

Artifical Intelligence v 1.0.1.4 ai (30.03.2017)
- Исправлена ошибка метода SearchFor : AI.cs
- структура TopItemPlaces : Vision.cs
- метод UpdateResource() : Vision.cs
- метод FindResourceByTheWay()  : AStarSolver.cs

Artifical Intelligence v 1.0.1.3 ai (30.03.2017)
- Изменен метод GoTo класса AStarSolver
- Начат метод FindResource класса AStarSolver
- Изменен метод ASolverMove класса AStarSolver, теперь он возращает объект Chain
- В структуре Place переопределен оператор +
- В структуре Place переопределен преобразование из типа Direction в тип Place
- Реализация метода SearchFor в классе AI
- Исправить ошибку метода SearchFor

Artifical Intelligence v 1.0.1.3 (30.03.2017)
- почти закончен SearchFor()

Artifical Intelligence v 1.0.1.2 (30.03.2017)
- создан UpdateMines()
- создан UpdateDwellings()

Artifical Intelligence v 1.0.1.1 (30.03.2017)
- упрощена структура Bottom
- добавлены списки mines, dwellings

Artifical Intelligence v 1.0.1.0 (30.03.2017)
- Структура Place перемещена из класса HoMM.Client.Program в класс AStarSolver
- Структура Chain перемещена из класса AStarSolver в пространство имен Homm.Client
- Добавлен метод Start в классе AI
- В класс AI теперь передается объект класса HommClient
- Изменен метод CanIHireUnit: теперь он возвращает точное количество юнитов, которое может приобрести игрок
- Найдена ошибка в инициализации bottom_map, исправлена, но ТАКОООООООЙ жесткий костыль :D (перемещайся по закладкам)
- Убрана заглушка в методе FillCurrentBottom
- Убраны лишние комментарии
- Удалены ненужные файлы из директории
- Добавлено описание прошлых обновлений
- Создан файл TODO для помещения туда идей

Artifical Intelligence v 1.0.0.2 (29.03.2017)
- метод CanIHireUnit (WantToBuy)
- возвращение работоспособности

Artifical Intelligence v 1.0.0.1 (29.03.2017)
- Изменили структуру Bottom
- Изменили структуру Top
- Новый метод FillCurrentBottom
- начало работы над ИИ:
	-myResourse
	-WantToBuy - переделать

Artifical Intelligence v 1.0.0.0 (29.03.2017)
- New method "UpdateBottom"
- Changed "InitBottom"
