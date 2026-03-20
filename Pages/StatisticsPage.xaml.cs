using CW_hammer.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CW_hammer.Pages
{
    // ── ViewModels ───────────────────────────────────────────────────────────
    public class TreatingRow
    {
        public string AnimalName { get; set; } = "";
        public string Species { get; set; } = "";
        public string OwnerName { get; set; } = "";
        public string DiseaseName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public string Complaint { get; set; } = "";
        // Термін лікування = сьогодні - дата початку
        public string DaysText => $"{(DateTime.Today - StartDate).Days} дн.";
    }

    public class CategoryRow
    {
        public string Species { get; set; } = "";
        public int Total { get; set; }
        public int Alive { get; set; }
        public int Treating { get; set; }
    }

    public class FinishedRow
    {
        public string AnimalName { get; set; } = "";
        public string Species { get; set; } = "";
        public string OwnerName { get; set; } = "";
        public string DiseaseName { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        // Тривалість = дата кінця - дата початку
        public string DaysText => $"{(EndDate - StartDate).Days} дн.";
    }

    // ── Сторінка ─────────────────────────────────────────────────────────────
    public partial class StatisticsPage : Page
    {
        private readonly AppDbContext _db;

        public StatisticsPage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            Loaded += async (_, _) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            // ── 1. Зараз лікуються (DiseaseState = true) ────────────────────
            var treating = await _db.MedicalHistories
                .Where(m => m.DiseaseState)
                .Include(m => m.AnimalCard).ThenInclude(a => a.PetOwner)
                .Include(m => m.DiseaseDirectory)
                .Select(m => new TreatingRow
                {
                    AnimalName = m.AnimalCard.Name,
                    Species = m.AnimalCard.Species ?? "—",
                    OwnerName = m.AnimalCard.PetOwner != null
                                    ? m.AnimalCard.PetOwner.FirstName + " "
                                      + m.AnimalCard.PetOwner.LastName
                                    : "—",
                    DiseaseName = m.DiseaseDirectory.Name,
                    StartDate = m.StartDate,
                    Complaint = m.Complaint ?? "—"
                })
                .ToListAsync();

            TreatingGrid.ItemsSource = treating;

            // ── 2. Кількість по видах ────────────────────────────────────────
            var activeAnimalIds = await _db.MedicalHistories
                .Where(m => m.DiseaseState)
                .Select(m => m.AnimalCardID)
                .ToListAsync();

            var allAnimals = await _db.AnimalCards.ToListAsync();

            var categories = allAnimals
                .GroupBy(a => a.Species ?? "Невідомо")
                .Select(g => new CategoryRow
                {
                    Species = g.Key,
                    Total = g.Count(),
                    Alive = g.Count(a => a.IsAlive),
                    Treating = g.Count(a => activeAnimalIds.Contains(a.ID))
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            CategoryGrid.ItemsSource = categories;

            // ── 3. Завершили лікування минулого місяця ───────────────────────────
            var today = DateTime.Today;
            var firstDay = new DateTime(today.Year, today.Month, 1).AddMonths(-1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Назва місяця українською
            var monthNames = new[]
            {
    "січень", "лютий", "березень", "квітень",
    "травень", "червень", "липень", "серпень",
    "вересень", "жовтень", "листопад", "грудень"
};
            var monthName = monthNames[firstDay.Month - 1];

            LastMonthTitle.Text = $"✅ Завершили лікування — {monthName} {firstDay.Year}";

            var finished = await _db.MedicalHistories
                .Where(m => !m.DiseaseState
                         && m.EndDate.HasValue
                         && m.EndDate.Value >= firstDay
                         && m.EndDate.Value <= lastDay)
                .Include(m => m.AnimalCard).ThenInclude(a => a.PetOwner)
                .Include(m => m.DiseaseDirectory)
                .Select(m => new FinishedRow
                {
                    AnimalName = m.AnimalCard.Name,
                    Species = m.AnimalCard.Species ?? "—",
                    OwnerName = m.AnimalCard.PetOwner != null
                                    ? m.AnimalCard.PetOwner.FirstName + " "
                                      + m.AnimalCard.PetOwner.LastName
                                    : "—",
                    DiseaseName = m.DiseaseDirectory.Name,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate!.Value
                })
                .ToListAsync();

            LastMonthGrid.ItemsSource = finished;
        }
    }
}