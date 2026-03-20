using CW_hammer.Data;
using CW_hammer.Models;
using CW_hammer.Themes;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CW_hammer.Pages
{
    public partial class HomePage : Page
    {
        private readonly AppDbContext _db;
        private List<(string Path, string Name)> _slides = [];
        private int _slideIndex = 0;
        private DispatcherTimer? _timer;

        public HomePage(AppDbContext db)
        {
            InitializeComponent();
            _db = db;
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _slides = await _db.AnimalCards
                .Where(a => a.IsAlive && a.Photos.Any())
                .Include(a => a.Photos)
                .SelectMany(a => a.Photos.Select(p => new { p.FilePath, a.Name }))
                .Select(x => ValueTuple.Create(x.FilePath, x.Name))
                .ToListAsync();

            if (_slides.Count == 0)
            {
                NoPhotoLabel.Visibility = Visibility.Visible;
                return;
            }

            ShowSlide(0);
            StartSlideshow();
        }

        private void ShowSlide(int index)
        {
            var (path, name) = _slides[index];
            SlideLabel.Text = name;
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(ResolvePath(path), UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                SlideImage.Source = bmp;
            }
            catch { SlideImage.Source = null; }
        }

        private void StartSlideshow()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(4) };
            _timer.Tick += (_, _) =>
            {
                _slideIndex = (_slideIndex + 1) % _slides.Count;
                ShowSlide(_slideIndex);
            };
            _timer.Start();
        }

        // ── Кнопки навігації ─────────────────────────────────────────────────
        private void PatientsBtn_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new PatientsPage(_db));

        private void StatsBtn_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new StatisticsPage(_db));

        private void NewPatientBtn_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate(new NewPatientPage(_db));

        // ── Тема — відкрити меню по кліку на кнопку ─────────────────────────
        private void ThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void Theme_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string tag
                && int.TryParse(tag, out int index))
            {
                ThemeManager.Apply((AppTheme)index);
            }
        }

        private void DiseaseDirectoryBtn_Click(object sender, RoutedEventArgs e)
    => NavigationService.Navigate(new DiseaseDirectoryPage(_db));

        // ── Хелпер ───────────────────────────────────────────────────────────
        private static string ResolvePath(string relativePath)
        {
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var normalized = relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            return System.IO.Path.Combine(exeDir, normalized);
        }
    }
}