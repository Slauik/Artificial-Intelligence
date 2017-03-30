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
