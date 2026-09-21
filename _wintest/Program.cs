using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KeyClickCounter;
using KeyClickCounter.ViewModels;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var app = new System.Windows.Application();
        // Mirror App.xaml merged dictionaries so the real MainWindow's StaticResource references resolve.
        app.Resources.MergedDictionaries.Add(new ResourceDictionary
            { Source = new Uri("pack://application:,,,/KeyClickCounter;component/Themes/Dark.xaml") });
        app.Resources.MergedDictionaries.Add(new ResourceDictionary
            { Source = new Uri("pack://application:,,,/KeyClickCounter;component/Resources/KeyboardLayout.xaml") });

        var window = new MainWindow();
        window.Loaded += async (_, _) =>
        {
            try
            {
                await Task.Delay(800); // let bindings + timers settle
                var vm = (KeyCountViewModel)window.DataContext;
                var pickers = VisualChildrenOfType<DatePicker>(window).ToList();
                var startPicker = pickers[0];
                var endPicker = pickers[1];

                var today = DateTime.Today;
                var startDate = vm.RangeStart; // storage floor (first-launch date)

                Console.WriteLine($"initial RangeStart={vm.RangeStart:yyyy-MM-dd} RangeEnd={vm.RangeEnd:yyyy-MM-dd} title=\"{window.Title}\"");
                Console.WriteLine();

                // (1) valid-ish end change: end = today-2
                endPicker.SelectedDate = today.AddDays(-2);
                await Task.Delay(400);
                Console.WriteLine($"END={today.AddDays(-2):yyyy-MM-dd}: VM={vm.RangeEnd:yyyy-MM-dd} picker={endPicker.SelectedDate:yyyy-MM-dd}");

                // (2) valid-ish start change: start = today-8
                startPicker.SelectedDate = today.AddDays(-8);
                await Task.Delay(400);
                Console.WriteLine($"START={today.AddDays(-8):yyyy-MM-dd}: VM={vm.RangeStart:yyyy-MM-dd} picker={startPicker.SelectedDate:yyyy-MM-dd}");

                // (3) push start up to the end boundary
                var endBefore = vm.RangeEnd;
                startPicker.SelectedDate = endBefore.AddDays(5);
                await Task.Delay(400);
                bool movedToEnd = vm.RangeStart == endBefore;
                Console.WriteLine($"START beyond end: VM={vm.RangeStart:yyyy-MM-dd} picker={startPicker.SelectedDate:yyyy-MM-dd} -> {(movedToEnd ? "PASS" : "FAIL")} (start moved to end)");

                // (4) CRITICAL: same out-of-range pick AGAIN -> clamped back to current value,
                //     the DatePicker must REVERT to the actual value (the bug: it kept the picked date).
                startPicker.SelectedDate = endBefore.AddDays(5);
                await Task.Delay(400);
                bool revertStart = startPicker.SelectedDate == vm.RangeStart;
                Console.WriteLine($"START beyond end (2nd): VM={vm.RangeStart:yyyy-MM-dd} picker={startPicker.SelectedDate:yyyy-MM-dd} -> {(revertStart ? "PASS" : "FAIL")} (picker reverted)");

                // (5) push end up to today
                endPicker.SelectedDate = today.AddDays(30);
                await Task.Delay(400);
                bool endIsToday = vm.RangeEnd == today;
                Console.WriteLine($"END future: VM={vm.RangeEnd:yyyy-MM-dd} picker={endPicker.SelectedDate:yyyy-MM-dd} -> {(endIsToday ? "PASS" : "FAIL")} (end clamped to today)");

                // (6) CRITICAL: same future-end pick AGAIN -> picker must revert to today
                endPicker.SelectedDate = today.AddDays(30);
                await Task.Delay(400);
                bool revertEnd = endPicker.SelectedDate == vm.RangeEnd;
                Console.WriteLine($"END future (2nd): VM={vm.RangeEnd:yyyy-MM-dd} picker={endPicker.SelectedDate:yyyy-MM-dd} -> {(revertEnd ? "PASS" : "FAIL")} (picker reverted)");

                // (7) push start down to the first-launch floor
                startPicker.SelectedDate = startDate.AddDays(-60);
                await Task.Delay(400);
                bool atFloor = vm.RangeStart == startDate;
                Console.WriteLine($"START before first-launch: VM={vm.RangeStart:yyyy-MM-dd} picker={startPicker.SelectedDate:yyyy-MM-dd} -> {(atFloor ? "PASS" : "FAIL")} (start clamped to floor)");

                // (8) CRITICAL: same floor pick AGAIN -> picker must revert to the floor
                startPicker.SelectedDate = startDate.AddDays(-60);
                await Task.Delay(400);
                bool revertFloor = startPicker.SelectedDate == vm.RangeStart;
                Console.WriteLine($"START before first-launch (2nd): VM={vm.RangeStart:yyyy-MM-dd} picker={startPicker.SelectedDate:yyyy-MM-dd} -> {(revertFloor ? "PASS" : "FAIL")} (picker reverted)");
                // final invariants: pickers + title + status bar must all agree with the VM range
                bool syncPickers = startPicker.SelectedDate == vm.RangeStart && endPicker.SelectedDate == vm.RangeEnd;
                bool titleOk = window.Title.Contains(vm.RangeStart.ToString("yyyy-MM-dd")) && window.Title.Contains(vm.RangeEnd.ToString("yyyy-MM-dd"));
                bool statusOk = vm.StatusText.Contains(vm.RangeStart.ToString("yyyy-MM-dd")) && vm.StatusText.Contains(vm.RangeEnd.ToString("yyyy-MM-dd"));
                Console.WriteLine((syncPickers ? "PASS" : "FAIL") + ": both pickers == VM range (no divergence)");
                Console.WriteLine((titleOk ? "PASS" : "FAIL") + ": window TITLE == VM range");
                Console.WriteLine((statusOk ? "PASS" : "FAIL") + ": STATUS BAR == VM range");

                bool ok = movedToEnd && endIsToday && atFloor && revertStart && revertEnd && revertFloor && syncPickers && titleOk && statusOk;
                Console.WriteLine(ok ? "== END-TO-END: OK ==" : "== END-TO-END: BUG PRESENT ==");
                Environment.Exit(ok ? 0 : 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("TEST EXCEPTION: " + ex);
                Environment.Exit(3);
            }
        };

        window.Show();
        app.Run();
    }

    private static IEnumerable<T> VisualChildrenOfType<T>(DependencyObject root) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T t) yield return t;
            foreach (var sub in VisualChildrenOfType<T>(child)) yield return sub;
        }
    }
}

