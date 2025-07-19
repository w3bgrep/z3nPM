using Microsoft.Maui.Controls;


namespace z3nDesk
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private int memLimit;
        private bool kill = false;

        private void ChekZen(object sender, EventArgs e)
        {
            string[] processNames = new[] { "ZennoPoster", "zbe1" }; // Укажите второе имя процесса
            int momoryLimit = (int)mLimit.Value; // Предполагается, что mLimit — Slider, возвращает double
            int timeLimit = 999;

            var allProcs = new List<System.Diagnostics.Process>();
            foreach (var processName in processNames)
            {
                allProcs.AddRange(System.Diagnostics.Process.GetProcessesByName(processName));
            }

            if (allProcs.Count == 0)
            {
                DisplayAlert("Title", "No processes by filter", "Ok");
            }
            else
            {
                string found = "";
                string killed = "";
                foreach (var proc in allProcs)
                {
                    DateTime StartTime = proc.StartTime;
                    TimeSpan Time_diff = DateTime.Now - StartTime;
                    int Time_diff_int = Convert.ToInt32(Time_diff.TotalMinutes);
                    long memoryUsage = proc.WorkingSet64 / (1024 * 1024);

                    found += $"{proc.ProcessName} {memoryUsage}Mb {Time_diff_int} min\n";

                    if (kill)
                    {
                        if (Time_diff_int > timeLimit || memoryUsage > momoryLimit)
                        {
                            proc.Kill();
                            killed += $"{proc.Id} {memoryUsage}Mb {Time_diff_int} min\n";
                        }
                    }
                }
                DisplayAlert("Title", $"Killed: \n{killed} \nFound: \n{found} ", "Ok");
            }
        }

        private void ShowBigTasks(object sender, EventArgs e)
        {
            int momoryLimit = memLimit;

            var procs = System.Diagnostics.Process.GetProcesses(); // Get all processes
            if (procs.Length == 0)
            {
                DisplayAlert("Title", "No processes found", "Ok");
            }
            else
            {
                string found = "";

                foreach (var proc in procs)
                {
                    try
                    {
                        DateTime StartTime = proc.StartTime;
                        TimeSpan Time_diff = DateTime.Now - StartTime;
                        int Time_diff_int = Convert.ToInt32(Time_diff.TotalMinutes);
                        long memoryUsage = proc.WorkingSet64 / (1024 * 1024);

                        if (memoryUsage > momoryLimit)
                        {
                            found += $"{proc.ProcessName} | {proc.Id} | {memoryUsage}Mb | {Time_diff_int} min\n";
                        }
                    }
                    catch (Exception)
                    {
                        // Skip processes that can't be accessed (e.g., system processes)
                        continue;
                    }
                }

                DisplayAlert("Title", string.IsNullOrEmpty(found) ? "No processes exceed memory limit" : $"Found: \n{found}", "Ok");
            }
        }

        private void MemLimitChanged(object sender, ValueChangedEventArgs e)
        {
            memLimit = Convert.ToInt32(e.NewValue);
            memoryLimit.Text = $"Memory Limit: {memLimit.ToString()}Mb";

            showAllButton.Text = $"Show tasks consumes more than:{memLimit.ToString()}"; 
            if (kill) killButton.Text = $"Kill Zenno tasks if consumed memory exeeds {memLimit}Mb";

        }

        private void Kill(object sender, ToggledEventArgs e)
        {
            kill = e.Value;
            killButton.Text = e.Value ? $"Kill Zenno tasks if consumed memory exeeds {memLimit}Mb" : "Show Zenno tasks";
            //killButton.Text = e.Value ? $"Kill" : "Show";
        }

        private void StartKillTimer(object sender, EventArgs e)
        {
            memoryLimit.Text = $"timer started at: {DateTime.Now:HH:mm:ss}\n";
            Device.StartTimer(TimeSpan.FromHours(1), () =>
            {
                ChekZen(null, EventArgs.Empty);
                memoryLimit.Text = $"Last ChekZen: {DateTime.Now:HH:mm:ss}\n";
                return true; 
            });
        }




    }
}
