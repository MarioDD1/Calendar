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
        public Form1()
        {
            this.Text = "Календарь";
            this.ClientSize = new System.Drawing.Size(700, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.White;
            lblMonth = new Label
            {
                Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold),
                Location = new System.Drawing.Point(150, 10),
                Size = new System.Drawing.Size(400, 40),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };
            lblMonth.Click += (s, e) => SelectMonthYear();
            btnPrev = new Button { Text = "<", Location = new System.Drawing.Point(10, 10), Size = new System.Drawing.Size(50, 40) };
            btnPrev.Click += (s, e) => ChangeMonth(-1);
            btnNext = new Button { Text = ">", Location = new System.Drawing.Point(640, 10), Size = new System.Drawing.Size(50, 40) };
            btnNext.Click += (s, e) => ChangeMonth(1);
            flowLayoutPanel1 = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 550,
                BackColor = System.Drawing.Color.WhiteSmoke,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                AutoScroll = true,
                Padding = new Padding(10)
            };
            this.Controls.Add(lblMonth);
            this.Controls.Add(btnPrev);
            this.Controls.Add(btnNext);
            this.Controls.Add(flowLayoutPanel1);
            this.FormClosing += Form1_FormClosing;
            _month = DateTime.Now.Month;
            _year = DateTime.Now.Year;
            LoadNotes();
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
                select.ClientSize = new System.Drawing.Size(300, 150);
                ComboBox cbMonth = new ComboBox { Location = new System.Drawing.Point(50, 20), Width = 200 };
                for (int i = 1; i <= 12; i++)
                    cbMonth.Items.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i));
                cbMonth.SelectedIndex = _month - 1;
                NumericUpDown nudYear = new NumericUpDown { Location = new System.Drawing.Point(50, 60), Width = 200, Minimum = 1900, Maximum = 2100, Value = _year };
                Button btnOk = new Button { Text = "OK", Location = new System.Drawing.Point(100, 100), Width = 80 };
                btnOk.Click += (sender, ev) =>
                {
                    _month = cbMonth.SelectedIndex + 1;
                    _year = (int)nudYear.Value;
                    DisplayDays(_month, _year);
                    select.Close();
                };
                select.Controls.Add(cbMonth);
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
            for (int i = 0; i < firstDayOfWeek; i++)
            {
                ucDays blank = new ucDays();
                blank.SetDay("");
                flowLayoutPanel1.Controls.Add(blank);
            }
            for (int d = 1; d <= days; d++)
            {
                ucDays uc = new ucDays();
                string key = $"{year}-{month:D2}-{d:D2}";
                uc.SetDay(d.ToString());

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
                if (d == DateTime.Now.Day && month == DateTime.Now.Month && year == DateTime.Now.Year)
                    uc.BackColor = Color.LightCyan;

                flowLayoutPanel1.Controls.Add(uc);
            }
        }
        private void LoadNotes()
        {
            if (File.Exists(saveFile))
            {
                string json = File.ReadAllText(saveFile);
                notes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            }
        }
        private void SaveNotes()
        {
            string json = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(saveFile, json);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveNotes();
        }
    }
}
