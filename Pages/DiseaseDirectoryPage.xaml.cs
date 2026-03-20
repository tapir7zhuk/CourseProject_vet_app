using CW_hammer.Data;
using CW_hammer.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CW_hammer.Pages
{
    public partial class DiseaseDirectoryPage : Page
    {
        private readonly AppDbContext _db;
        private int? _editId;

        public DiseaseDirectoryPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            Loaded += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            var list = await _db.DiseaseDirectories
                .Where(d => d.ID != 1001)   // приховуємо системний "Огляд"
                .OrderBy(d => d.Name)
                .ToListAsync();
            DiseaseGrid.ItemsSource = list;
        }

        // ── Зберегти (додати або редагувати) ────────────────────────────────
        private async void SaveDisease_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            { MessageBox.Show("Вкажіть назву"); return; }

            if (_editId.HasValue)
            {
                var record = await _db.DiseaseDirectories.FindAsync(_editId.Value);
                if (record == null) return;
                record.Name = NameBox.Text.Trim();
                record.Category = CategoryBox.Text.Trim();
            }
            else
            {
                _db.DiseaseDirectories.Add(new DiseaseDirectory
                {
                    Name = NameBox.Text.Trim(),
                    Category = CategoryBox.Text.Trim()
                });
            }

            await _db.SaveChangesAsync();
            ClearForm();
            await LoadAsync();
        }

        // ── Редагувати (з контекстного меню) ────────────────────────────────
        private void EditDisease_Click(object sender, RoutedEventArgs e)
        {
            if (DiseaseGrid.SelectedItem is not DiseaseDirectory row) return;

            _editId = row.ID;
            NameBox.Text = row.Name;
            CategoryBox.Text = row.Category ?? "";
            FormTitle.Text = $"✏️ Редагувати: {row.Name}";
        }

        // ── Видалити ─────────────────────────────────────────────────────────
        private async void DeleteDisease_Click(object sender, RoutedEventArgs e)
        {
            if (DiseaseGrid.SelectedItem is not DiseaseDirectory row) return;

            var result = MessageBox.Show(
                $"Видалити '{row.Name}'?",
                "Підтвердження",
                MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes) return;

            var record = await _db.DiseaseDirectories.FindAsync(row.ID);
            if (record == null) return;

            _db.DiseaseDirectories.Remove(record);
            await _db.SaveChangesAsync();
            await LoadAsync();
        }

        // ── Скасувати редагування ────────────────────────────────────────────
        private void CancelEdit_Click(object sender, RoutedEventArgs e)
            => ClearForm();

        private void ClearForm()
        {
            _editId = null;
            NameBox.Text = "";
            CategoryBox.Text = "";
            FormTitle.Text = "➕ Додати хворобу";
        }
    }
}