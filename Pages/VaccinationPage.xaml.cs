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
    public partial class VaccinationPage : Page
    {
        private readonly AppDbContext _db;
        private readonly int _animalId;
        private readonly int? _editId;

        // Конструктор редагування
        public VaccinationPage(AppDbContext db, int animalId)
        {
            InitializeComponent();
            _db = db;
            _animalId = animalId;
            _editId = null;
            Loaded += async (_, _) => await LoadAsync();
            VaccineDatePicker.SelectedDate = DateTime.Today;
            ValidUntilPicker.SelectedDate = DateTime.Today.AddYears(1);
        }

        // ── Редагування ──────────────────────────────────────────────────
        public VaccinationPage(AppDbContext db, int animalId, int editId)
        {
            InitializeComponent();
            _db = db;
            _animalId = animalId;
            _editId = editId;
            Loaded += async (_, _) => await LoadAsync();
        }


        private async Task LoadAsync()
        {
            var animal = await _db.AnimalCards
                .Include(a => a.PetOwner)
                .Include(a => a.Photos)
                .FirstOrDefaultAsync(a => a.ID == _animalId);

            if (animal == null) return;

            PatientName.Text = animal.Name;
            PatientOwner.Text = animal.PetOwner != null
                ? $"{animal.PetOwner.FirstName} {animal.PetOwner.LastName}" : "—";
            PatientSpecies.Text = animal.Species ?? "—";

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

            if (_editId.HasValue)
            {
                var vacc = await _db.Vaccinations.FindAsync(_editId.Value);
                if (vacc != null)
                {
                    ManufacturerBox.Text = vacc.Manufacturer ?? "";
                    PurposeBox.Text = vacc.Purpose ?? "";
                    SerialBox.Text = vacc.SerialNumber ?? "";
                    VaccineDatePicker.SelectedDate = vacc.VaccinationDate;
                    ValidUntilPicker.SelectedDate = vacc.ValidUntil;
                }
            }
            }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ManufacturerBox.Text))
            { MessageBox.Show("Вкажіть виробника"); return; }
            if (SerialBox.Text.Length != 6 || !SerialBox.Text.All(char.IsDigit))
            { MessageBox.Show("Серійний номер — рівно 6 цифр"); return; }
            if (VaccineDatePicker.SelectedDate == null || ValidUntilPicker.SelectedDate == null)
            { MessageBox.Show("Вкажіть дати"); return; }

            if (_editId.HasValue)
            {
                // Редагування
                var record = await _db.Vaccinations.FindAsync(_editId.Value);
                if (record == null) return;
                record.Manufacturer = ManufacturerBox.Text.Trim();
                record.Purpose = PurposeBox.Text.Trim();
                record.SerialNumber = SerialBox.Text.Trim();
                record.VaccinationDate = VaccineDatePicker.SelectedDate.Value;
                record.ValidUntil = ValidUntilPicker.SelectedDate.Value;
            }
            else
            {
                // Новий запис
                _db.Vaccinations.Add(new Vaccination
                {
                    AnimalCardID = _animalId,
                    Manufacturer = ManufacturerBox.Text.Trim(),
                    Purpose = PurposeBox.Text.Trim(),
                    SerialNumber = SerialBox.Text.Trim(),
                    VaccinationDate = VaccineDatePicker.SelectedDate.Value,
                    ValidUntil = ValidUntilPicker.SelectedDate.Value
                });
            }

            await _db.SaveChangesAsync();
            MessageBox.Show("✅ Збережено");
            NavigationService.GoBack();
        }

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