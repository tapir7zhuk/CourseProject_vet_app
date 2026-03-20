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
    public partial class EditAnimalPage : Page
    {
        private readonly AppDbContext _db;
        private readonly int _animalId;

        public EditAnimalPage(AppDbContext db, int animalId)
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
                .FirstOrDefaultAsync(a => a.ID == _animalId);

            if (animal == null) return;

            // Фото
            var photo = animal.Photos?.FirstOrDefault();
            if (photo != null)
            {
                AnimalNoPhoto.Visibility = Visibility.Collapsed;
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new Uri(ResolvePath(photo.FilePath), UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    AnimalPhoto.Source = bmp;
                }
                catch { AnimalPhoto.Source = null; }
            }

            AnimalNameLabel.Text = animal.Name;
            AnimalOwnerLabel.Text = animal.PetOwner != null
                ? $"{animal.PetOwner.FirstName} {animal.PetOwner.LastName}"
                : "—";

            // Заповнюємо поля
            NameBox.Text = animal.Name;
            SpeciesBox.Text = animal.Species ?? "";
            BreedBox.Text = animal.Breed ?? "";
            ColoringBox.Text = animal.Coloring ?? "";
            BirthDatePicker.SelectedDate = animal.BirthDate;

            // Стать
            SexCombo.SelectedIndex = (int)animal.Sex;

            // Стерилізація
            SterileCombo.SelectedIndex = animal.Sterile ? 1 : 0;

            // Живий
            AliveCombo.SelectedIndex = animal.IsAlive ? 0 : 1;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            { MessageBox.Show("Вкажіть кличку"); return; }

            var animal = await _db.AnimalCards.FindAsync(_animalId);
            if (animal == null) return;

            animal.Name = NameBox.Text.Trim();
            animal.Species = SpeciesBox.Text.Trim();
            animal.Breed = BreedBox.Text.Trim();
            animal.Coloring = ColoringBox.Text.Trim();
            animal.BirthDate = BirthDatePicker.SelectedDate ?? animal.BirthDate;

            if (SexCombo.SelectedItem is ComboBoxItem sexItem)
                animal.Sex = (AnimalCards.SexEnum)int.Parse(sexItem.Tag.ToString()!);

            if (SterileCombo.SelectedItem is ComboBoxItem sterileItem)
                animal.Sterile = sterileItem.Tag.ToString() == "true";

            if (AliveCombo.SelectedItem is ComboBoxItem aliveItem)
                animal.IsAlive = aliveItem.Tag.ToString() == "true";

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