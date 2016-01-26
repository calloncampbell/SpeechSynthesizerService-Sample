using System;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.Reflection;

using RS.Threading.Worker;
using ReflectSoftware.Insight;

namespace SpeechSynthesizerService.TestClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ReflectInsight ri;
        WorkManager wManager;

        public MainWindow()
        {
            InitializeComponent();

            // Create ReflectInsight Object for Logging.
            ri = new ReflectInsight("SpeechSynthesizerService.TestClient");

            // Initialize the WorkManager
            wManager = new WorkManager();            
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            ri.AddSeparator();
            ri.AddSeparator();
            ri.AddSeparator();

            using (ri.TraceMethod(MethodBase.GetCurrentMethod().Name))
            {
                btnStart.IsEnabled = false;

                wManager.Start();                
                
                btnStop.IsEnabled = true;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            using (ri.TraceMethod(MethodBase.GetCurrentMethod().Name))
            {
                btnStop.IsEnabled = false;

                wManager.Stop();

                btnStart.IsEnabled = true;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
