using CW_hammer.Data;
using CW_hammer.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CW_hammer.Pages
{
    public partial class MedHistoryPage : Page
    {
        private readonly AppDbContext _db;
        private readonly int _animalId;
        private readonly bool _isExam;
        private readonly int? _editId;  // ← додано

        // Звичайна хвороба
        public MedHistoryPage(AppDbContext db, int animalId)
        {
            InitializeComponent();
            _db = db;
            _animalId = animalId;
            _isExam = false;
            _editId = null;
            Loaded += async (_, _) => await LoadAsync();
        }

        // Огляд
        public MedHistoryPage(AppDbContext db, int animalId, bool isExam)
        {
            InitializeComponent();
            _db = db;
            _animalId = animalId;
            _isExam = isExam;
            _editId = null;
            Loaded += async (_, _) => await LoadAsync();
        }

        // Редагування ← новий конструктор
        public MedHistoryPage(AppDbContext db, int animalId, int editId)
        {
            InitializeComponent();
            _db = db;
            _animalId = animalId;
            _isExam = false;
            _editId = editId;
            Loaded += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            // ── Дані пацієнта ─────────────────────────────────────────────
            var animal = await _db.AnimalCards
                .Include(a => a.PetOwner)
                .Include(a => a.Photos)
                .FirstOrDefaultAsync(a => a.ID == _animalId);

            if (animal == null) return;

            PatientName.Text = animal.Name;
            PatientOwner.Text = animal.PetOwner != null
                                    ? $"{animal.PetOwner.FirstName} {animal.PetOwner.LastName}"
                                    : "—";
            PatientSpecies.Text = animal.Species ?? "—";
            PatientSex.Text = animal.Sex switch
            {
                AnimalCards.SexEnum.Male => "Самець",
                AnimalCards.SexEnum.Female => "Самиця",
                _ => "Невідомо"
            };

            var photo = animal.Photos?.FirstOrDefault();
            if (photo != null)
            {
                PatientNoPhoto.Visibility = Visibility.Collapsed;
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(ResolvePath(photo.FilePath), UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    PatientPhoto.Source = bmp;
                }
                catch { PatientPhoto.Source = null; }
            }

            // ── Режим редагування ─────────────────────────────────────────
            if (_editId.HasValue)
            {
                FormTitle.Text = "✏️ Редагувати хворобу";

                var diseases = await _db.DiseaseDirectories
                    .Where(d => d.ID != 1001)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                DiseaseCombo.IsEnabled = true;
                DiseaseCombo.ItemsSource = diseases;

                var record = await _db.MedicalHistories.FindAsync(_editId.Value);
                if (record != null)
                {
                    DiseaseCombo.SelectedValue = record.DiseaseDirectoryID;
                    ComplaintBox.Text = record.Complaint ?? "";
                    PrescriptionBox.Text = record.Prescription ?? "";
                    StartDatePicker.SelectedDate = record.StartDate;
                    EndDatePicker.SelectedDate = record.EndDate;
                    StateCombo.SelectedIndex = record.DiseaseState ? 0 : 1;
                }
            }
            // ── Режим Огляд ───────────────────────────────────────────────
            else if (_isExam)
            {
                FormTitle.Text = "📋 Огляд";
                ComplaintLabel.Text = "Огляд";
                PrescriptionLabel.Text = "Стан пацієнта";

                DiseaseCombo.IsEnabled = false;
                var examDisease = await _db.DiseaseDirectories
                    .FirstOrDefaultAsync(d => d.ID == 1001);
                if (examDisease != null)
                {
                    DiseaseCombo.ItemsSource = new[] { examDisease };
                    DiseaseCombo.SelectedIndex = 0;
                }

                ComplaintBox.Text = "Огляд";
                PrescriptionBox.Text = "Здоровий";
                StartDatePicker.SelectedDate = DateTime.Today;
            }
            // ── Режим Хвороба ─────────────────────────────────────────────
            else
            {
                FormTitle.Text = "🏥 Додати хворобу";

                var diseases = await _db.DiseaseDirectories
                    .Where(d => d.ID != 1001)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                DiseaseCombo.IsEnabled = true;
                DiseaseCombo.ItemsSource = diseases;
                StartDatePicker.SelectedDate = DateTime.Today;
            }
        }

        // ── Зберегти ──────────────────────────────────────────────────────
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DiseaseCombo.SelectedValue == null)
            { MessageBox.Show("Оберіть хворобу"); return; }
            if (StartDatePicker.SelectedDate == null)
            { MessageBox.Show("Вкажіть дату початку"); return; }

            bool isActive = StateCombo.SelectedItem is ComboBoxItem item
                            && item.Tag?.ToString() == "true";

            if (_editId.HasValue)
            {
                // ── Редагування ───────────────────────────────────────────
                var record = await _db.MedicalHistories.FindAsync(_editId.Value);
                if (record == null) return;

                record.DiseaseDirectoryID = (int)DiseaseCombo.SelectedValue;
                record.StartDate = StartDatePicker.SelectedDate.Value;
                record.EndDate = EndDatePicker.SelectedDate;
                record.DiseaseState = isActive;
                record.Complaint = ComplaintBox.Text.Trim();
                record.Prescription = PrescriptionBox.Text.Trim();
            }
            else
            {
                // ── Новий запис ───────────────────────────────────────────
                _db.MedicalHistories.Add(new MedicalHistory
                {
                    AnimalCardID = _animalId,
                    DiseaseDirectoryID = (int)DiseaseCombo.SelectedValue,
                    StartDate = StartDatePicker.SelectedDate.Value,
                    EndDate = EndDatePicker.SelectedDate,
                    DiseaseState = isActive,
                    Complaint = ComplaintBox.Text.Trim(),
                    Prescription = PrescriptionBox.Text.Trim()
                });
            }

            await _db.SaveChangesAsync();
            MessageBox.Show("✅ Збережено");
            NavigationService.GoBack();
        }

        // ── Відмінити ─────────────────────────────────────────────────────
        private void Cancel_Click(object sender, RoutedEventArgs e)
            => NavigationService.GoBack();

        private static string ResolvePath(string relativePath)
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var normalized = relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            return System.IO.Path.Combine(exeDir, normalized);
        }
    }
}