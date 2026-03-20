using CW_hammer.Data;
using CW_hammer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CW_hammer.Pages
{
    public class PatientRow
    {
        public int ID { get; set; }
        public string OwnerFullName { get; set; } = "";
        public string Name { get; set; } = "";
        public string Species { get; set; } = "";
        public string StatusText { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Address { get; set; } = "";
        public string Breed { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public AnimalCards.SexEnum Sex { get; set; }
        public List<string> Photos { get; set; } = [];
    }

    public partial class PatientsPage : Page
    {
        private readonly AppDbContext _db;
        private PatientRow? _selectedPatient;
        private List<PatientRow> _allRows = [];

        public PatientsPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            Loaded += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            var activeIds = await _db.MedicalHistories
                .Where(m => m.DiseaseState)
                .Select(m => m.AnimalCardID)
                .ToListAsync();

            var rows = await _db.AnimalCards
                .Include(a => a.PetOwner)
                .Include(a => a.Photos)
                .Select(a => new PatientRow
                {
                    ID = a.ID,
                    Name = a.Name,
                    Species = a.Species ?? "—",
                    Breed = a.Breed ?? "—",
                    OwnerFullName = a.PetOwner != null
                                    ? a.PetOwner.FirstName + " " + a.PetOwner.LastName
                                    : "—",
                    Phone = a.PetOwner != null ? a.PetOwner.Phone ?? "—" : "—",
                    Address = a.PetOwner != null ? a.PetOwner.Address ?? "—" : "—",
                    BirthDate = a.BirthDate,
                    Sex = a.Sex,
                    Photos = a.Photos.Select(p => p.FilePath).ToList(),
                    StatusText = !a.IsAlive ? "💀 Помер" :
                                    activeIds.Contains(a.ID) ? "🏥 Лікується" :
                                    "✅ Здоровий"
                })
                .ToListAsync();

            _allRows = rows;
            PatientsGrid.ItemsSource = _allRows;
        }

        // ── Наведення ────────────────────────────────────────────────────────
        private void PatientsGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement fe &&
                fe.DataContext is PatientRow row &&
                row != _selectedPatient)
            {
                UpdateExplorer(row);
            }
        }

        private void PatientsGrid_Click(object sender, MouseButtonEventArgs e)
        {
            if (PatientsGrid.SelectedItem is PatientRow row)
                UpdateExplorer(row);
        }

        private void UpdateExplorer(PatientRow row)
        {
            _selectedPatient = row;
            ExplorerHint.Visibility = Visibility.Collapsed;
            ExplorerContent.Visibility = Visibility.Visible;

            EPhone.Text = row.Phone;
            EAddress.Text = row.Address;
            EAge.Text = CalcAge(row.BirthDate);
            ESex.Text = row.Sex switch
            {
                AnimalCards.SexEnum.Male => "Самець",
                AnimalCards.SexEnum.Female => "Самиця",
                _ => "Невідомо"
            };
            EBreed.Text = row.Breed;

            if (row.Photos.Count > 0)
            {
                ExplorerNoPhoto.Visibility = Visibility.Collapsed;
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(ResolvePath(row.Photos[0]), UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    ExplorerPhoto.Source = bmp;
                }
                catch { ExplorerPhoto.Source = null; }
            }
            else
            {
                ExplorerPhoto.Source = null;
                ExplorerNoPhoto.Visibility = Visibility.Visible;
            }
        }

        // ── Кнопки експлорера ────────────────────────────────────────────────
        private void MedCard_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPatient == null) return;
            OpenInBrowser(new MedCardPage(_db, _selectedPatient.ID),
                          $"📄 {_selectedPatient.Name}");
        }

        // ── Контекстне меню ──────────────────────────────────────────────────
        private void CtxMedCard_Click(object sender, RoutedEventArgs e)
        {
            if (PatientsGrid.SelectedItem is not PatientRow row) return;
            OpenInBrowser(new MedCardPage(_db, row.ID), $"📄 {row.Name}");
        }

        private void CtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPatient == null)
            {
                if (PatientsGrid.SelectedItem is not PatientRow row) return;
                NavigationService.Navigate(new EditAnimalPage(_db, row.ID));
                return;
            }
            NavigationService.Navigate(new EditAnimalPage(_db, _selectedPatient.ID));
        }

        // ── Нижні кнопки ─────────────────────────────────────────────────────
        private void AddTreatment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPatient == null)
            { MessageBox.Show("Оберіть пацієнта"); return; }
            NavigationService.Navigate(new MedHistoryPage(_db, _selectedPatient.ID));
        }

        private void AddExam_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPatient == null)
            { MessageBox.Show("Оберіть пацієнта"); return; }
            NavigationService.Navigate(
                new MedHistoryPage(_db, _selectedPatient.ID, isExam: true));
        }

        private void NewPatient_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new NewPatientPage(_db));

        // ── Хелпери ──────────────────────────────────────────────────────────
        private void OpenInBrowser(Page page, string title)
        {
            var browser = Window.GetWindow(this) as BrowserWindow;
            browser?.OpenTab(title, page);
        }

        private static string ResolvePath(string relativePath)
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var normalized = relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            return System.IO.Path.Combine(exeDir, normalized);
        }

        private static string CalcAge(DateTime birth)
        {
            var t = DateTime.Today;
            int y = t.Year - birth.Year, m = t.Month - birth.Month;
            if (m < 0) { y--; m += 12; }
            return y > 0 ? $"{y} р. {m} міс." : $"{m} міс.";
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
                SearchBox.Focus();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var q = SearchBox.Text;
            PatientsGrid.ItemsSource = string.IsNullOrWhiteSpace(q)
                ? _allRows
                : [.. _allRows.Where(r =>
                    r.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    r.OwnerFullName.Contains(q, StringComparison.OrdinalIgnoreCase))];
        }
    }
}