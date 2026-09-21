using System.ComponentModel;
using System.Windows.Threading;
using KeyClickCounter.Models;
using KeyClickCounter.ViewModels;

// Drive the ViewModel exactly like the DatePicker bindings would (RangeStart / RangeEnd setters).
var vm = new KeyCountViewModel();
var changes = new List<string>();
vm.PropertyChanged += (_, e) => changes.Add(e.PropertyName);

vm.InitializeFromStorage(new StorageData
{
    Version = 2,
    StartDate = "2026-09-01",
    Days = new Dictionary<string, Dictionary<string, long>>
    {
        ["2026-09-01"] = new() { ["A"] = 10, ["MouseWheel"] = 100, ["OtherCount"] = 3 },
        ["2026-09-03"] = new() { ["A"] = 5, ["B"] = 7, ["MouseWheel"] = 50 },
    },
});

Console.WriteLine("== after InitializeFromStorage ==");
Console.WriteLine("RangeStart=" + vm.RangeStart.ToString("yyyy-MM-dd"));
Console.WriteLine("RangeEnd  =" + vm.RangeEnd.ToString("yyyy-MM-dd"));
Console.WriteLine("StatusText=" + vm.StatusText);
Console.WriteLine();

// User picks a NEW start date in the toolbar DatePicker (query condition changes).
changes.Clear();
vm.RangeStart = new DateTime(2026, 9, 3);
Console.WriteLine("== after RangeStart = 2026-09-03 ==");
Console.WriteLine("RangeStart=" + vm.RangeStart.ToString("yyyy-MM-dd"));
Console.WriteLine("StatusText=" + vm.StatusText);
Console.WriteLine("PropertyChanged raised: " + string.Join(", ", changes));
Console.WriteLine();

// User picks a new END date too.
changes.Clear();
vm.RangeEnd = new DateTime(2026, 9, 5);
Console.WriteLine("== after RangeEnd = 2026-09-05 ==");
Console.WriteLine("RangeEnd  =" + vm.RangeEnd.ToString("yyyy-MM-dd"));
Console.WriteLine("StatusText=" + vm.StatusText);
Console.WriteLine("PropertyChanged raised: " + string.Join(", ", changes));
Console.WriteLine();

// Verify the window-title/status-bar source string contains the NEW start date.
bool ok = vm.StatusText.Contains("2026-09-03");
Console.WriteLine((ok ? "PASS" : "FAIL") + ": StatusText reflects new start date (title + status bar would update).");
Environment.Exit(ok ? 0 : 1);
