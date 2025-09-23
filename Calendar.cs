using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

class CalendarProgram
{
    // Хранилище данных
    private List<Note> notes;           // Список всех заметок
    private DateTime currentDate;       // Текущий отображаемый месяц
    private DateTime selectedDate;      // Выбранная пользователем дата
    private List<Holiday> holidays;     // Список праздников
    private string notesFilePath = "notes.json"; // Файл для сохранения заметок

    // инициализация при запуске программы
    public CalendarProgram()
    {
        notes = new List<Note>();
        currentDate = DateTime.Today;
        selectedDate = DateTime.Today;
        holidays = InitializeHolidays(); 
        LoadNotes(); // Загружаем сохраненные заметки
    }

    // Главный цикл программы
    public void Run()
    {
        while (true)
        {
            Console.Clear(); // Очищаем консоль перед каждым обновлением
            DisplayCalendar(); // Рисуем календарь
            DisplayMenu(); // Показываем меню управления
            
            Console.Write("Выберите действие: ");
            string input = Console.ReadLine();
            
            if (!string.IsNullOrEmpty(input))
            {
                HandleInput(input.ToUpper()); // Обрабатываем ввод пользователя
            }
        }
    }

    // Отображение календаря в консоли
    private void DisplayCalendar()
    {
        // Шапка с информацией о датах
        Console.WriteLine($"=== КАЛЕНДАРЬ ===\n");
        Console.WriteLine($"Сегодня: {DateTime.Today:dd.MM.yyyy}");
        Console.WriteLine($"Выбранная дата: {selectedDate:dd.MM.yyyy}");
        Console.WriteLine($"Текущий месяц: {GetRussianMonthName(currentDate)} {currentDate.Year}\n");

        // Заголовок дней недели
        string[] daysOfWeek = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };
        Console.WriteLine(" " + string.Join(" ", daysOfWeek));

        // Вычисляем первый и последний день текущего месяца
        DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        // Определяем день недели первого дня месяца (1-7, где 1-понедельник, 7-воскресенье)
        int firstDayWeekday = (int)firstDayOfMonth.DayOfWeek;
        if (firstDayWeekday == 0) firstDayWeekday = 7; // Воскресенье -> 7

        // Создаем отступ для первого дня (чтобы он попал на правильный день недели)
        Console.Write(new string(' ', (firstDayWeekday - 1) * 3));

        // Проходим по всем дням месяца и отображаем их
        for (DateTime day = firstDayOfMonth; day <= lastDayOfMonth; day = day.AddDays(1))
        {
            // Проверяем различные состояния дня
            bool isToday = day.Date == DateTime.Today.Date;
            bool isSelected = day.Date == selectedDate.Date;
            bool isWeekend = IsWeekend(day);
            bool isHoliday = IsHoliday(day);
            bool hasNote = notes.Any(n => n.Date.Date == day.Date);

            // Устанавливаем цвета в зависимости от состояния дня
            if (isSelected && isToday)
            {
                // Если день и выбранный и сегодняшний - фиолетовый
                Console.BackgroundColor = ConsoleColor.Magenta;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (isSelected)
            {
                // Выбранный день - синий
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (isToday)
            {
                // Сегодняшний день - зеленый
                Console.BackgroundColor = ConsoleColor.Green;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (isHoliday)
            {
                // Праздничный день - желтый
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.ForegroundColor = ConsoleColor.Black;
            }
            else if (isWeekend)
            {
                // Выходной день - красный текст
                Console.ForegroundColor = ConsoleColor.Red;
            }
            else
            {
                // Обычный рабочий день - белый текст
                Console.ForegroundColor = ConsoleColor.White;
            }

            // Форматируем отображение дня
            string dayDisplay;
            if (isSelected)
            {
                // Выбранный день показываем в скобках
                dayDisplay = hasNote ? $"[{day.Day,2}]" : $"({day.Day,2})";
            }
            else
            {
                // Обычное отображение дня
                dayDisplay = hasNote ? $"[{day.Day,2}]" : $"{day.Day,2}";
            }
            
            Console.Write(dayDisplay);
            Console.ResetColor(); // Сбрасываем цвета после вывода дня
            Console.Write(" ");

            // Переход на новую строку после воскресенья
            if (day.DayOfWeek == DayOfWeek.Sunday)
            {
                Console.WriteLine();
            }
        }

        Console.WriteLine("\n");
        Console.ResetColor();

        // Легенда цветов для пользователя
        Console.WriteLine("Пометки:");
        
        
        Console.Write("  [ ] - Заметки");
        Console.Write("  ( ) - Курсор");
        Console.WriteLine("\n");

        // Дополнительная информация
        DisplayNotesForSelectedDate();  // Заметки выбранной даты
        DisplayHolidaysForCurrentMonth(); // Праздники текущего месяца
    }

    // Получение русского названия месяца
    private string GetRussianMonthName(DateTime date)
    {
        string[] months = {
            "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь",
            "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
        };
        return months[date.Month - 1];
    }

    // Показ заметок для выбранной даты
    private void DisplayNotesForSelectedDate()
    {
        // Ищем все заметки для выбранной даты
        var dateNotes = notes.Where(n => n.Date.Date == selectedDate.Date).ToList();
        if (dateNotes.Any())
        {
            Console.WriteLine("Заметки на выбранную дату:");
            foreach (var note in dateNotes)
            {
                Console.WriteLine($"  • {note.Text}"); // Выводим каждую заметку
            }
            Console.WriteLine();
        }
    }

    // Показ праздников текущего месяца
    private void DisplayHolidaysForCurrentMonth()
    {
        // Фильтруем праздники по текущему месяцу и году
        var monthHolidays = holidays
            .Where(h => h.Date.Year == currentDate.Year && h.Date.Month == currentDate.Month)
            .OrderBy(h => h.Date) // Сортируем по дате
            .ToList();

        if (monthHolidays.Any())
        {
            Console.WriteLine("Праздники этого месяца:");
            foreach (var holiday in monthHolidays)
            {
                // Помечаем официальные и традиционные праздники
                string holidayType = holiday.IsOfficial ? "(офиц.)" : "(трад.)";
                Console.WriteLine($"  • {holiday.Date:dd.MM} {holidayType} - {holiday.Name}");
            }
            Console.WriteLine();
        }
    }

    // Отображение меню управления
    private void DisplayMenu()
    {
        Console.WriteLine("Управление:");
        Console.WriteLine("1 - Предыдущий месяц");
        Console.WriteLine("2 - Следующий месяц");
        Console.WriteLine("3 - Выбрать предыдущую неделю");
        Console.WriteLine("4 - Выбрать следующую неделю");
        Console.WriteLine("5 - Выбрать конкретную дату");
        Console.WriteLine("6 - Добавить заметку");
        Console.WriteLine("7 - Удалить заметку");
        Console.WriteLine("8 - Перейти к сегодняшней дате");
        Console.WriteLine("9 - Показать рабочие/выходные дни и праздники");
        Console.WriteLine("0 - Выход");
        Console.WriteLine();
    }

    // Обработка ввода пользователя
    private void HandleInput(string input)
    {
        switch (input)
        {
            case "1":
                currentDate = currentDate.AddMonths(-1); // Переход на предыдущий месяц
                break;

            case "2":
                currentDate = currentDate.AddMonths(1); // Переход на следующий месяц
                break;

            case "3":
                // Переход на неделю назад
                selectedDate = selectedDate.AddDays(-7);
                if (selectedDate.Month != currentDate.Month)
                {
                    currentDate = selectedDate; // Обновляем текущий месяц если вышли за его пределы
                }
                break;

            case "4":
                // Переход на неделю вперед
                selectedDate = selectedDate.AddDays(7);
                if (selectedDate.Month != currentDate.Month)
                {
                    currentDate = selectedDate;
                }
                break;

            case "5":
                SelectSpecificDate(); // Ручной ввод даты
                break;

            case "6":
                AddNote(); // Добавление заметки
                break;

            case "7":
                DeleteNote(); // Удаление заметки
                break;

            case "8":
                // Возврат к сегодняшней дате
                currentDate = DateTime.Today;
                selectedDate = DateTime.Today;
                break;

            case "9":
                ShowWorkWeekendHolidayInfo(); // Статистика по дням
                break;

            case "0":
                SaveNotes(); // Сохраняем заметки перед выходом
                Environment.Exit(0); // Завершение программы
                break;

            default:
                // Попытка распознать ввод как дату
                if (DateTime.TryParse(input, out DateTime newDate))
                {
                    selectedDate = newDate;
                    currentDate = newDate;
                }
                else
                {
                    Console.WriteLine("Неверная команда! Нажмите Enter для продолжения...");
                    Console.ReadLine();
                }
                break;
        }
    }

    // Функция ручного выбора даты
    private void SelectSpecificDate()
    {
        Console.Clear();
        Console.WriteLine("=== ВЫБОР ДАТЫ ===\n");
        
        // Ввод года
        Console.Write("Введите год (например 2024): ");
        if (!int.TryParse(Console.ReadLine(), out int year) || year < 1900 || year > 2100)
        {
            Console.WriteLine("Неверный год! Нажмите Enter для продолжения...");
            Console.ReadLine();
            return;
        }

        // Ввод месяца
        Console.Write("Введите месяц (1-12): ");
        if (!int.TryParse(Console.ReadLine(), out int month) || month < 1 || month > 12)
        {
            Console.WriteLine("Неверный месяц! Нажмите Enter для продолжения...");
            Console.ReadLine();
            return;
        }

        // Показываем доступные дни для выбранного месяца
        int daysInMonth = DateTime.DaysInMonth(year, month);
        Console.WriteLine($"\nДоступные дни в месяце: 1-{daysInMonth}");
        Console.Write("Введите день: ");
        
        // Ввод дня с проверкой корректности
        if (!int.TryParse(Console.ReadLine(), out int day) || day < 1 || day > daysInMonth)
        {
            Console.WriteLine($"Неверный день! Должен быть от 1 до {daysInMonth}");
            Console.ReadLine();
            return;
        }

        try
        {
            // Создаем новую дату и устанавливаем ее
            DateTime newDate = new DateTime(year, month, day);
            selectedDate = newDate;
            currentDate = newDate;
            Console.WriteLine($"Дата установлена: {newDate:dd.MM.yyyy}");
            Console.ReadLine();
        }
        catch
        {
            Console.WriteLine("Неверная дата! Нажмите Enter для продолжения...");
            Console.ReadLine();
        }
    }

    // Добавление новой заметки
    private void AddNote()
    {
        Console.Write("Введите заметку: ");
        string noteText = Console.ReadLine();
        
        if (!string.IsNullOrWhiteSpace(noteText))
        {
            // Проверяем, нет ли уже заметки на эту дату
            var existingNote = notes.FirstOrDefault(n => n.Date.Date == selectedDate.Date);
            if (existingNote != null)
            {
                // Запрос подтверждения замены существующей заметки
                Console.Write($"На эту дату уже есть заметка: '{existingNote.Text}'. Заменить? (y/n): ");
                string answer = Console.ReadLine();
                if (answer?.ToLower() != "y")
                {
                    return; // Отмена если пользователь не подтвердил
                }
                notes.Remove(existingNote); // Удаляем старую заметку
            }
            
            // Добавляем новую заметку и сохраняем
            notes.Add(new Note(selectedDate, noteText));
            SaveNotes();
            Console.WriteLine("Заметка добавлена! Нажмите Enter для продолжения...");
            Console.ReadLine();
        }
    }

    // Удаление заметки
    private void DeleteNote()
    {
        // Ищем все заметки для выбранной даты
        var dateNotes = notes.Where(n => n.Date.Date == selectedDate.Date).ToList();
        if (!dateNotes.Any())
        {
            Console.WriteLine("Нет заметок для удаления! Нажмите Enter для продолжения...");
            Console.ReadLine();
            return;
        }

        // Показываем список заметок для удаления
        Console.WriteLine("Выберите заметку для удаления:");
        for (int i = 0; i < dateNotes.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {dateNotes[i].Text}");
        }

        Console.Write("Введите номер заметки (0 - отмена): ");
        if (int.TryParse(Console.ReadLine(), out int choice))
        {
            if (choice == 0) return; // Отмена операции
            
            if (choice >= 1 && choice <= dateNotes.Count)
            {
                // Удаляем выбранную заметку
                var noteToRemove = dateNotes[choice - 1];
                notes.Remove(noteToRemove);
                SaveNotes(); // Сохраняем изменения
                Console.WriteLine("Заметка удалена! Нажмите Enter для продолжения...");
            }
            else
            {
                Console.WriteLine("Неверный выбор! Нажмите Enter для продолжения...");
            }
        }
        Console.ReadLine();
    }

    // Показ статистики по рабочим/выходным дням и праздникам
    private void ShowWorkWeekendHolidayInfo()
    {
        Console.Clear();
        Console.WriteLine("=== РАБОЧИЕ ДНИ, ВЫХОДНЫЕ И ПРАЗДНИКИ ===\n");

        // Определяем границы текущего месяца
        DateTime startDate = new DateTime(currentDate.Year, currentDate.Month, 1);
        DateTime endDate = startDate.AddMonths(1).AddDays(-1);

        int workDays = 0;
        int weekendDays = 0;
        int holidayDays = 0;

        Console.WriteLine($"Месяц: {GetRussianMonthName(currentDate)} {currentDate.Year}");
        Console.WriteLine("\nРабочие дни (Пн-Пт, не праздники):");
        
        // Считаем и показываем рабочие дни
        for (DateTime day = startDate; day <= endDate; day = day.AddDays(1))
        {
            if (!IsWeekend(day) && !IsHoliday(day))
            {
                Console.Write($"{day:dd.MM} ");
                workDays++;
            }
        }

        Console.WriteLine("\n\nВыходные дни (Сб-Вс, не праздники):");
        // Считаем и показываем выходные дни
        for (DateTime day = startDate; day <= endDate; day = day.AddDays(1))
        {
            if (IsWeekend(day) && !IsHoliday(day))
            {
                Console.Write($"{day:dd.MM} ");
                weekendDays++;
            }
        }

        Console.WriteLine("\n\nПраздничные дни:");
        // Показываем праздники месяца
        var monthHolidays = holidays
            .Where(h => h.Date.Year == currentDate.Year && h.Date.Month == currentDate.Month)
            .OrderBy(h => h.Date)
            .ToList();

        foreach (var holiday in monthHolidays)
        {
            string holidayType = holiday.IsOfficial ? "офиц." : "трад.";
            Console.WriteLine($"  {holiday.Date:dd.MM} ({holidayType}) - {holiday.Name}");
            holidayDays++;
        }

        // Выводим статистику
        Console.WriteLine($"\nСтатистика:");
        Console.WriteLine($"• Рабочих дней: {workDays}");
        Console.WriteLine($"• Выходных дней: {weekendDays}");
        Console.WriteLine($"• Праздничных дней: {holidayDays}");
        Console.WriteLine($"• Всего дней в месяце: {DateTime.DaysInMonth(currentDate.Year, currentDate.Month)}");

        // Показываем ближайшие праздники (на 3 месяца вперед)
        var upcomingHolidays = holidays
            .Where(h => h.Date >= DateTime.Today && h.Date <= DateTime.Today.AddMonths(3))
            .OrderBy(h => h.Date)
            .Take(5)
            .ToList();

        if (upcomingHolidays.Any())
        {
            Console.WriteLine($"\nБлижайшие праздники:");
            foreach (var holiday in upcomingHolidays)
            {
                // Рассчитываем сколько дней осталось до праздника
                string daysLeft = (holiday.Date - DateTime.Today).Days == 0 ? 
                    "Сегодня" : $"через {(holiday.Date - DateTime.Today).Days} дн.";
                Console.WriteLine($"  {holiday.Date:dd.MM.yyyy} - {holiday.Name} ({daysLeft})");
            }
        }

        Console.WriteLine("\nНажмите Enter для возврата...");
        Console.ReadLine();
    }

    // Загрузка заметок из файла
    private void LoadNotes()
    {
        try
        {
            if (File.Exists(notesFilePath))
            {
                // Читаем JSON из файла
                string json = File.ReadAllText(notesFilePath);
                // Десериализуем в промежуточную структуру
                var noteDataList = JsonSerializer.Deserialize<List<NoteData>>(json);
                
                // Преобразуем обратно в объекты Note
                notes = noteDataList.Select(nd => new Note(
                    new DateTime(nd.Year, nd.Month, nd.Day), 
                    nd.Text
                )).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка загрузки заметок: {ex.Message}");
        }
    }

    // Сохранение заметок в файл
    private void SaveNotes()
    {
        try
        {
            // Преобразуем заметки в упрощенную структуру для сериализации
            var noteDataList = notes.Select(n => new NoteData
            {
                Year = n.Date.Year,
                Month = n.Date.Month,
                Day = n.Date.Day,
                Text = n.Text
            }).ToList();

            // Сериализуем в JSON с красивым форматированием
            string json = JsonSerializer.Serialize(noteDataList, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(notesFilePath, json); // Записываем в файл
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сохранения заметок: {ex.Message}");
        }
    }

    // Проверка является ли день выходным
    private bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    // Проверка является ли день праздником
    private bool IsHoliday(DateTime date)
    {
        return holidays.Any(h => h.Date.Date == date.Date);
    }

    // Инициализация списка праздников
    private List<Holiday> InitializeHolidays()
    {
        var holidaysList = new List<Holiday>();
        
        // Добавляем праздники для нескольких лет (2023-2026)
        for (int year = 2023; year <= 2026; year++)
        {
            // Официальные государственные праздники России
            holidaysList.Add(new Holiday(new DateTime(year, 1, 1), "Новый год", true));
            holidaysList.Add(new Holiday(new DateTime(year, 1, 2), "Новогодние каникулы", true));
            holidaysList.Add(new Holiday(new DateTime(year, 1, 3), "Новогодние каникулы", true));
            holidaysList.Add(new Holiday(new DateTime(year, 1, 4), "Новогодние каникулы", true));
            holidaysList.Add(new Holiday(new DateTime(year, 1, 5), "Новогодние каникулы", true));
            holidaysList.Add(new Holiday(new DateTime(year, 1, 6), "Новогодние каникулы", true));
            holidaysList.Add(new Holiday(new DateTime(year, 1, 7), "Рождество Христово", true));
            holidaysList.Add(new Holiday(new DateTime(year, 1, 8), "Новогодние каникулы", true));
            
            holidaysList.Add(new Holiday(new DateTime(year, 2, 23), "День защитника Отечества", true));
            holidaysList.Add(new Holiday(new DateTime(year, 3, 8), "Международный женский день", true));
            holidaysList.Add(new Holiday(new DateTime(year, 5, 1), "Праздник Весны и Труда", true));
            holidaysList.Add(new Holiday(new DateTime(year, 5, 9), "День Победы", true));
            holidaysList.Add(new Holiday(new DateTime(year, 6, 12), "День России", true));
            holidaysList.Add(new Holiday(new DateTime(year, 11, 4), "День народного единства", true));

            // Традиционные праздники (неофициальные)
            holidaysList.Add(new Holiday(new DateTime(year, 2, 14), "День святого Валентина", false));
            holidaysList.Add(new Holiday(new DateTime(year, 2, 25), "День тестя", false));
            holidaysList.Add(new Holiday(new DateTime(year, 4, 1), "День смеха", false));
            holidaysList.Add(new Holiday(new DateTime(year, 4, 12), "День космонавтики", false));
            holidaysList.Add(new Holiday(new DateTime(year, 5, 24), "День славянской письменности", false));
            holidaysList.Add(new Holiday(new DateTime(year, 6, 1), "День защиты детей", false));
            holidaysList.Add(new Holiday(new DateTime(year, 7, 8), "День семьи, любви и верности", false));
            holidaysList.Add(new Holiday(new DateTime(year, 9, 1), "День знаний", false));
            holidaysList.Add(new Holiday(new DateTime(year, 10, 5), "День учителя", false));
            holidaysList.Add(new Holiday(new DateTime(year, 12, 31), "Канун Нового года", false));
        }

        return holidaysList;
    }
}

// Класс для хранения заметки
class Note
{
    public DateTime Date { get; set; }  // Дата заметки
    public string Text { get; set; }    // Текст заметки
    
    public Note(DateTime date, string text)
    {
        Date = date.Date; // Сохраняем только дату без времени
        Text = text;
    }
}

// Вспомогательный класс для сериализации заметок
class NoteData
{
    public int Year { get; set; }   // Год заметки
    public int Month { get; set; }  // Месяц заметки
    public int Day { get; set; }    // День заметки
    public string Text { get; set; } // Текст заметки
}

// Класс для хранения информации о празднике
class Holiday
{
    public DateTime Date { get; set; }      // Дата праздника
    public string Name { get; set; }        // Название праздника
    public bool IsOfficial { get; set; }    // Является ли официальным праздником
    
    public Holiday(DateTime date, string name, bool isOfficial = true)
    {
        Date = date.Date; // Сохраняем только дату
        Name = name;
        IsOfficial = isOfficial;
    }
}

// Главный класс программы
class Program
{
    // Точка входа в программу
    static void Main(string[] args)
    {
        // Настройка консоли
        Console.Title = "Календарь с заметками и праздниками";
        Console.CursorVisible = true;
        
        try
        {
            // Создаем и запускаем календарь
            CalendarProgram calendar = new CalendarProgram();
            calendar.Run();
        }
        catch (Exception ex)
        {
            // Обработка критических ошибок
            Console.WriteLine($"Ошибка: {ex.Message}");
            Console.ReadLine();
        }
    }
}
