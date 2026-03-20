using CW_hammer.Data;
using CW_hammer.Models;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CW_hammer.Pages
{
    public partial class NewPatientPage : Page
    {
        private readonly AppDbContext _db;
        private string? _selectedPhotoPath;

        public NewPatientPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
        }

        private void ChoosePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Зображення|*.jpg;*.jpeg;*.png;*.bmp",
                Title = "Обрати фото"
            };
            if (dlg.ShowDialog() != true) return;

            _selectedPhotoPath = dlg.FileName;
            NoPhotoHint.Visibility = Visibility.Collapsed;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(_selectedPhotoPath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                PhotoPreview.Source = bmp;
            }
            catch { PhotoPreview.Source = null; }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валідація
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            { MessageBox.Show("Вкажіть кличку тварини"); return; }
            if (string.IsNullOrWhiteSpace(OwnerFirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(OwnerLastNameBox.Text))
            { MessageBox.Show("Вкажіть ім'я та прізвище власника"); return; }

            // Стать
            var sex = AnimalCards.SexEnum.Undefined;
            if (SexCombo.SelectedItem is ComboBoxItem sexItem)
                sex = (AnimalCards.SexEnum)int.Parse(sexItem.Tag.ToString()!);

            // Стерилізація
            bool sterile = false;
            if (SterileCombo.SelectedItem is ComboBoxItem sterileItem)
                sterile = sterileItem.Tag.ToString() == "true";

            // Створюємо власника
            var owner = new PetOwner
            {
                FirstName = OwnerFirstNameBox.Text.Trim(),
                LastName = OwnerLastNameBox.Text.Trim(),
                Phone = OwnerPhoneBox.Text.Trim(),
                Address = OwnerAddressBox.Text.Trim(),
                City = OwnerCityBox.Text.Trim(),
                ZipCode = OwnerZipBox.Text.Trim()
            };
            _db.PetOwner.Add(owner);
            await _db.SaveChangesAsync();

            // Створюємо тварину
            var animal = new AnimalCards
            {
                PetOwnerID = owner.ID,
                Name = NameBox.Text.Trim(),
                Species = SpeciesBox.Text.Trim(),
                Breed = BreedBox.Text.Trim(),
                Coloring = ColoringBox.Text.Trim(),
                BirthDate = BirthDatePicker.SelectedDate ?? DateTime.Today,
                Sex = sex,
                Sterile = sterile,
                IsAlive = true
            };
            _db.AnimalCards.Add(animal);
            await _db.SaveChangesAsync();

            // Фото
            if (!string.IsNullOrEmpty(_selectedPhotoPath))
            {
                _db.AnimalPhotos.Add(new AnimalPhoto
                {
                    AnimalCardID = animal.ID,
                    FilePath = _selectedPhotoPath
                });
                await _db.SaveChangesAsync();
            }

            MessageBox.Show("✅ Картку створено");
            NavigationService.GoBack();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
            => NavigationService.GoBack();
    }
}