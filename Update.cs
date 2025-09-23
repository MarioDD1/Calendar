using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace CalendarApp
{
    public class Form1 : Form
    {
        private int _month;
        private int _year;
        private Dictionary<string, string> notes = new Dictionary<string, string>();
        private string saveFile = "notes.json";
        private FlowLayoutPanel flowLayoutPanel1;
        private Label lblMonth;
        private Button btnPrev;
        private Button btnNext;
        private Button btnToday;

        public Form1()
        {
            // Изменение размера и внешнего вида приложения
            this.Text = "✨ Мой Календарь ✨";
            this.ClientSize = new System.Drawing.Size(900, 700);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 245, 255);
            this.Font = new Font("Segoe UI", 10F);

            // Создание панели для заголовка
            Panel headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(70, 130, 180) // Стальной синий
            };
            btnPrev = new Button 
            { 
                Text = "◀ Предыдущий", 
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(20, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnPrev.Click += (s, e) => ChangeMonth(-1);

            btnNext = new Button 
            { 
                Text = "Следующий ▶", 
                Size = new System.Drawing.Size(100, 40),
                Location = new System.Drawing.Point(780, 20),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnNext.Click += (s, e) => ChangeMonth(1);

            btnToday = new Button 
            { 
                Text = "Сегодня", 
                Size = new System.Drawing.Size(80, 40),
                Location = new System.Drawing.Point(410, 20),
                BackColor = Color.FromArgb(255, 215, 0), // Золотой
                FlatStyle = FlatStyle.Flat
            };
            btnToday.Click += (s, e) => GoToToday();

            // Заголовок месяца и года
            lblMonth = new Label
            {
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                Size = new System.Drawing.Size(400, 50),
                Location = new System.Drawing.Point(250, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            lblMonth.Click += (s, e) => SelectMonthYear();

            // Добавление элементов в панель заголовка
            headerPanel.Controls.Add(lblMonth);
            headerPanel.Controls.Add(btnPrev);
            headerPanel.Controls.Add(btnNext);
            headerPanel.Controls.Add(btnToday);

            // Панель дней календаря
            flowLayoutPanel1 = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(255, 255, 255), // Белый фон
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                Padding = new Padding(15),
                Margin = new Padding(10)
            };

            // Создание панели для дней недели
            Panel daysOfWeekPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(100, 150, 200) // Голубой
            };

            // Добавление подписей дней недели
            string[] daysOfWeek = { "ПН", "ВТ", "СР", "ЧТ", "ПТ", "СБ", "ВС" };
            int dayWidth = 110; 

            for (int i = 0; i < 7; i++)
            {
                Label dayLabel = new Label
                {
                    Text = daysOfWeek[i],
                    Size = new Size(dayWidth, 30),
                    Location = new Point(15 + i * dayWidth, 5),
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.White
                };
                daysOfWeekPanel.Controls.Add(dayLabel);
            }

            // Добавление элементов на форму
            this.Controls.Add(flowLayoutPanel1);
            this.Controls.Add(daysOfWeekPanel);
            this.Controls.Add(headerPanel);

            this.FormClosing += Form1_FormClosing;
            _month = DateTime.Now.Month;
            _year = DateTime.Now.Year;
            LoadNotes();
            DisplayDays(_month, _year);
        }

        private void GoToToday()
        {
            _month = DateTime.Now.Month;
            _year = DateTime.Now.Year;
            DisplayDays(_month, _year);
        }

        private void ChangeMonth(int delta)
        {
            _month += delta;
            if (_month < 1) { _month = 12; _year--; }
            if (_month > 12) { _month = 1; _year++; }
            DisplayDays(_month, _year);
        }

        private void SelectMonthYear()
        {
            using (Form select = new Form())
            {
                select.Text = "Выберите месяц и год";
                select.FormBorderStyle = FormBorderStyle.FixedDialog;
                select.StartPosition = FormStartPosition.CenterParent;
                select.ClientSize = new System.Drawing.Size(300, 180);
                select.BackColor = Color.FromArgb(240, 245, 255);
                
                Label monthLabel = new Label { Text = "Месяц:", Location = new Point(20, 20), AutoSize = true };
                ComboBox cbMonth = new ComboBox { Location = new Point(80, 20), Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };
                
                for (int i = 1; i <= 12; i++)
                    cbMonth.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
                cbMonth.SelectedIndex = _month - 1;

                Label yearLabel = new Label { Text = "Год:", Location = new Point(20, 60), AutoSize = true };
                NumericUpDown nudYear = new NumericUpDown 
                { 
                    Location = new Point(80, 60), 
                    Width = 180, 
                    Minimum = 1900, 
                    Maximum = 2100, 
                    Value = _year 
                };

                Button btnOk = new Button 
                { 
                    Text = "OK", 
                    Location = new Point(110, 100), 
                    Width = 80,
                    BackColor = Color.FromArgb(70, 130, 180),
                    ForeColor = Color.White
                };
                
                btnOk.Click += (sender, ev) =>
                {
                    _month = cbMonth.SelectedIndex + 1;
                    _year = (int)nudYear.Value;
                    DisplayDays(_month, _year);
                    select.Close();
                };

                select.Controls.Add(monthLabel);
                select.Controls.Add(cbMonth);
                select.Controls.Add(yearLabel);
                select.Controls.Add(nudYear);
                select.Controls.Add(btnOk);

                select.ShowDialog();
            }
        }

        private void DisplayDays(int month, int year)
        {
            flowLayoutPanel1.Controls.Clear();
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            lblMonth.Text = $"{monthName.ToUpper()} {year}";
            
            DateTime startOfMonth = new DateTime(year, month, 1);
            int days = DateTime.DaysInMonth(year, month);
            int firstDayOfWeek = (int)startOfMonth.DayOfWeek;
            
            // Корректировка для понедельника как первого дня недели
            firstDayOfWeek = firstDayOfWeek == 0 ? 6 : firstDayOfWeek - 1;

            // Пустые ячейки для дней предыдущего месяца
            for (int i = 0; i < firstDayOfWeek; i++)
            {
                ucDays blank = new ucDays();
                blank.SetDay("");
                blank.BackColor = Color.FromArgb(245, 245, 245); // Серый фон для пустых ячеек
                flowLayoutPanel1.Controls.Add(blank);
            }

            // Ячейки с днями текущего месяца
            for (int d = 1; d <= days; d++)
            {
                ucDays uc = new ucDays();
                string key = $"{year}-{month:D2}-{d:D2}";
                uc.SetDay(d.ToString());

                // Стилизация выходных дней
                DateTime currentDate = new DateTime(year, month, d);
                if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    uc.BackColor = Color.FromArgb(255, 240, 240); // Светло-красный для выходных
                }

                if (notes.ContainsKey(key))
                    uc.SetNote(notes[key]);
                
                uc.DayDoubleClicked += (s, e) =>
                {
                    string input = Interaction.InputBox("Введите событие:", "Событие", notes.ContainsKey(key) ? notes[key] : "");
                    if (!string.IsNullOrWhiteSpace(input))
                    {
                        notes[key] = input;
                        uc.SetNote(input);
                        SaveNotes();
                    }
                };

                // Выделение текущего дня
                if (d == DateTime.Now.Day && month == DateTime.Now.Month && year == DateTime.Now.Year)
                {
                    uc.BackColor = Color.FromArgb(255, 255, 200); // Светло-желтый для сегодняшнего дня
                    uc.BorderStyle = BorderStyle.FixedSingle;
                }

                flowLayoutPanel1.Controls.Add(uc);
            }
        }

        private void LoadNotes()
        {
            if (File.Exists(saveFile))
            {
                try
                {
                    string json = File.ReadAllText(saveFile);
                    notes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки заметок: {ex.Message}");
                }
            }
        }

        private void SaveNotes()
        {
            try
            {
                string json = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения заметок: {ex.Message}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveNotes();
        }
    }
}
