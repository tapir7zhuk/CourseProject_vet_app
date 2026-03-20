using CW_hammer.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CW_hammer
{
    public class BrowserTab : INotifyPropertyChanged
    {
        private string _title = "Нова вкладка";
        private bool _isActive;

        public string Title
        {
            get => _title;
            set { _title = value; PropertyChanged?.Invoke(this, new(nameof(Title))); }
        }

        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; PropertyChanged?.Invoke(this, new(nameof(IsActive))); }
        }

        public Page? Page { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public partial class BrowserWindow : Window
    {
        public ObservableCollection<BrowserTab> Tabs { get; } = [];
        private readonly IServiceProvider _sp;

        // Словник — щоб не дублювати таби для одного типу сторінки
        private static readonly Dictionary<Type, string> _pageTitles = new()
        {
            { typeof(HomePage),      "🏠 Головна"          },
            { typeof(PatientsPage),  "📋 Список пацієнтів" },
            { typeof(MedCardPage),   "📄 Мед картка"       },
            { typeof(EditAnimalPage), "✏️ Редагування" },
            { typeof(NewPatientPage),  "➕ Нова картка"  },
            { typeof(VaccinationPage), "💉 Щеплення"     },
            { typeof(DiseaseDirectoryPage), "📋 Довідник хвороб" },
        };

        public BrowserWindow(IServiceProvider sp)
        {
            InitializeComponent();
            DataContext = this;
            _sp = sp;

            var home = _sp.GetRequiredService<HomePage>();
            OpenTab("🏠 Головна", home);
        }

        // ── Відкрити або переключитись на таб ───────────────────────────────
        public void OpenTab(string title, Page page)
        {
            // Якщо таб з такою сторінкою вже є — просто активуємо
            var existing = Tabs.FirstOrDefault(t => t.Page?.GetType() == page.GetType()
                                                  && t.Page == page);
            if (existing != null)
            {
                ActivateTab(existing);
                return;
            }

            var tab = new BrowserTab { Title = title, Page = page };
            Tabs.Add(tab);
            ActivateTab(tab);
        }

        private void ActivateTab(BrowserTab tab)
        {
            // Знімаємо виділення з усіх
            foreach (var t in Tabs) t.IsActive = false;

            tab.IsActive = true;

            if (tab.Page != null)
                MainFrame.Navigate(tab.Page);
        }

        // ── Клік по табу ────────────────────────────────────────────────────
        private void Tab_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is BrowserTab tab)
                ActivateTab(tab);
        }

        // ── Коли Frame навігує (через NavigationService.Navigate з Page) ────
        private void MainFrame_Navigated(object sender,
            System.Windows.Navigation.NavigationEventArgs e)
        {
            BackBtn.IsEnabled = MainFrame.CanGoBack;
            ForwardBtn.IsEnabled = MainFrame.CanGoForward;

            if (e.Content is not Page page) return;

            // Перевіряємо чи є вже таб для цієї сторінки
            var existing = Tabs.FirstOrDefault(t => t.Page == page);
            if (existing != null)
            {
                foreach (var t in Tabs) t.IsActive = false;
                existing.IsActive = true;
                return;
            }

            // Нова сторінка — додаємо таб
            var title = _pageTitles.TryGetValue(page.GetType(), out var t2)
                        ? t2
                        : page.Title ?? "Нова вкладка";

            var newTab = new BrowserTab { Title = title, Page = page, IsActive = true };
            foreach (var t in Tabs) t.IsActive = false;
            Tabs.Add(newTab);
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is BrowserTab tab)
            {
                // Не закриваємо якщо це останній таб
                if (Tabs.Count == 1) return;

                var wasActive = tab.IsActive;
                var index = Tabs.IndexOf(tab);

                Tabs.Remove(tab);

                // Якщо закрили активний — переключаємось на сусідній
                if (wasActive)
                {
                    var newIndex = Math.Min(index, Tabs.Count - 1);
                    ActivateTab(Tabs[newIndex]);
                }
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack) MainFrame.GoBack();
        }

        private void ForwardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoForward) MainFrame.GoForward();
        }
    }
}
