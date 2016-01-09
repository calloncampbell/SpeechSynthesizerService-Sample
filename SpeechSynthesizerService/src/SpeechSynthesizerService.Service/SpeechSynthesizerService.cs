using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Reflection;

using RS.Threading.Worker;
using ReflectSoftware.Insight;

namespace WindowsService1
{
    public partial class SpeechSynthesizerService : ServiceBase
    {
        ReflectInsight ri;
        WorkManager wManager;

        public SpeechSynthesizerService()
        {
            InitializeComponent();

            // Create ReflectInsight Object for Logging.
            ri = new ReflectInsight("Speech Synthesizer Service");

            // Initialize the WorkManager
            wManager = new WorkManager();
        }

        protected override void OnStart(string[] args)
        {
            ri.AddSeparator();
            ri.AddSeparator();
            ri.AddSeparator();

            using (ri.TraceMethod(MethodBase.GetCurrentMethod().Name))
            {
                wManager.Start();
            }
        }

        protected override void OnStop()
        {
            using (ri.TraceMethod(MethodBase.GetCurrentMethod().Name))
            {
                wManager.Stop();
            }
        }
    }
}
