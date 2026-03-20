using CW_hammer.Data;
using CW_hammer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CW_hammer.Pages
{
    // ViewModels для таблиць
    public class AnimalCardRow
    {
        public string Name { get; set; } = "";
        public string Species { get; set; } = "";
        public string Breed { get; set; } = "";
        public string Coloring { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public string SexText { get; set; } = "";
        public string SterileText { get; set; } = "";
        public string AliveText { get; set; } = "";
    }

    public class DiseaseRow
    {
        public int ID { get; set; } 
        public string DiseaseName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public string EndDate { get; set; } = "";
        public string StateText { get; set; } = "";
    }
    public class VaccinationRow
    {
        public int ID { get; set; }
        public string Manufacturer { get; set; } = "";
        public string Purpose { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public DateTime VaccinationDate { get; set; }
        public DateTime ValidUntil { get; set; }
    }
    public partial class MedCardPage : Page
    {
        private readonly AppDbContext _db;
        private readonly int _animalId;

        public MedCardPage(AppDbContext db, int animalId)
        {
            InitializeComponent();
            _db = db;
            _animalId = animalId;
            Loaded += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            var animal = await _db.AnimalCards
                .Include(a => a.PetOwner)
                .Include(a => a.Photos)
                .Include(a => a.Vaccinations)
                .FirstOrDefaultAsync(a => a.ID == _animalId);

            if (animal == null) return;

            // ── Таблиця 1 — тварина ──────────────────────────────────────
            AnimalTable.ItemsSource = new[]
            {
                new AnimalCardRow
                {
                    Name        = animal.Name,
                    Species     = animal.Species  ?? "—",
                    Breed       = animal.Breed    ?? "—",
                    Coloring    = animal.Coloring ?? "—",
                    BirthDate   = animal.BirthDate,
                    SexText     = animal.Sex switch
                    {
                        AnimalCards.SexEnum.Male   => "Самець",
                        AnimalCards.SexEnum.Female => "Самиця",
                        _                          => "—"
                    },
                    SterileText = animal.Sterile ? "Так" : "Ні",
                    AliveText   = animal.IsAlive  ? "✅" : "💀"
                }
            };

            // ── Таблиця 2 — власник ──────────────────────────────────────
            if (animal.PetOwner != null)
                OwnerTable.ItemsSource = new[] { animal.PetOwner };

            // ── Таблиця 3 — щеплення ─────────────────────────────────────
            VaccinationTable.ItemsSource = animal.Vaccinations?.Select(v => new VaccinationRow
            {
                ID = v.ID,
                Manufacturer = v.Manufacturer ?? "—",
                Purpose = v.Purpose ?? "—",
                SerialNumber = v.SerialNumber ?? "—",
                VaccinationDate = v.VaccinationDate,
                ValidUntil = v.ValidUntil
            }).ToList();

            // ── Таблиця 4 — хвороби ──────────────────────────────────────
            var history = await _db.MedicalHistories
                .Include(m => m.DiseaseDirectory)
                .Where(m => m.AnimalCardID == _animalId)
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();

            DiseaseTable.ItemsSource = history.Select(m => new DiseaseRow
            {
                ID = m.ID,
                DiseaseName = m.DiseaseDirectory?.Name ?? "—",
                StartDate = m.StartDate,
                EndDate = m.EndDate.HasValue
                                ? m.EndDate.Value.ToString("dd.MM.yyyy")
                                : "—",
                StateText = m.DiseaseState ? "🔴 Активна" : "✅ Завершена"
            }).ToList();

            // ── Фото ─────────────────────────────────────────────────────
            var firstPhoto = animal.Photos?.FirstOrDefault();
            if (firstPhoto != null)
            {
                CardNoPhoto.Visibility = Visibility.Collapsed;
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(ResolvePath(firstPhoto.FilePath), UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    CardPhoto.Source = bmp;
                }
                catch { CardPhoto.Source = null; }
            }
        }

        private static string ResolvePath(string relativePath)
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            return System.IO.Path.Combine(exeDir, relativePath);
        }

        // ── Змінити фото ─────────────────────────────────────────────────
        private async void ChangePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Зображення|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Обрати фото"
            };

            if (dlg.ShowDialog() != true) return;

            // Зберігаємо шлях у БД
            var existing = await _db.AnimalPhotos
                .FirstOrDefaultAsync(p => p.AnimalCardID == _animalId);

            if (existing != null)
                existing.FilePath = dlg.FileName;
            else
                _db.AnimalPhotos.Add(new AnimalPhoto
                {
                    AnimalCardID = _animalId,
                    FilePath = dlg.FileName
                });

            await _db.SaveChangesAsync();
            await LoadAsync(); // оновлюємо сторінку
        }

        private void AddDisease_Click(object sender, RoutedEventArgs e)
    => NavigationService.Navigate(new MedHistoryPage(_db, _animalId));

        // ── Змінити стан хвороби ─────────────────────────────────────────────
        private async void DiseaseChangeState_Click(object sender, RoutedEventArgs e)
        {
            if (DiseaseTable.SelectedItem is not DiseaseRow row) return;

            var record = await _db.MedicalHistories.FindAsync(row.ID);
            if (record == null) return;

            // Перемикаємо стан
            record.DiseaseState = !record.DiseaseState;

            // Якщо завершуємо — ставимо дату
            if (!record.DiseaseState && record.EndDate == null)
                record.EndDate = DateTime.Today;

            // Якщо знову активна — прибираємо дату завершення
            if (record.DiseaseState)
                record.EndDate = null;

            await _db.SaveChangesAsync();
            await LoadAsync();  // оновлюємо сторінку
        }

        // ── Видалити хворобу ─────────────────────────────────────────────────
        private async void DiseaseDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DiseaseTable.SelectedItem is not DiseaseRow row) return;

            var result = MessageBox.Show(
                $"Видалити запис '{row.DiseaseName}'?",
                "Підтвердження",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes) return;

            var record = await _db.MedicalHistories.FindAsync(row.ID);
            if (record == null) return;

            _db.MedicalHistories.Remove(record);
            await _db.SaveChangesAsync();
            await LoadAsync();
        }

        // ── Редагувати хворобу ────────────────────────────────────────────────
        private void DiseaseEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DiseaseTable.SelectedItem is not DiseaseRow row) return;
            NavigationService.Navigate(new MedHistoryPage(_db, _animalId, row.ID));
        }

        // ── Редагувати тварину ────────────────────────────────────────────────
        private void EditAnimal_Click(object sender, RoutedEventArgs e)
        {
            var page = new EditAnimalPage(_db, _animalId);
            NavigationService.Navigate(page);
        }

        private void AddVaccination_Click(object sender, RoutedEventArgs e)
    => NavigationService.Navigate(new VaccinationPage(_db, _animalId));

        private async void VaccDelete_Click(object sender, RoutedEventArgs e)
        {
            if (VaccinationTable.SelectedItem is not VaccinationRow row) return;
            var result = MessageBox.Show($"Видалити щеплення '{row.Purpose}'?",
                "Підтвердження", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            var record = await _db.Vaccinations.FindAsync(row.ID);
            if (record == null) return;
            _db.Vaccinations.Remove(record);
            await _db.SaveChangesAsync();
            await LoadAsync();
        }

        private void VaccEdit_Click(object sender, RoutedEventArgs e)
        {
            if (VaccinationTable.SelectedItem is not VaccinationRow row) return;
            NavigationService.Navigate(new VaccinationPage(_db, _animalId, row.ID));
        }
    }
}