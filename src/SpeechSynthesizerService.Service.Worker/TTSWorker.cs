using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Media.SpeechSynthesis;

using RS.Threading.Worker;
using ReflectSoftware.Insight;
using System.Data.Entity;

namespace SpeechSynthesizerService.Service.Worker
{
    public class TTSWorker : BaseWorker
    {
        ReflectInsight ri;
        SpeechSynthesizer synthesizer;
        IReadOnlyList<VoiceInformation> voices;

        #region Worker Interface Methods

        /// <summary>
        /// Called when initializing worker.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();

            // Create ReflectInsight Object for Logging.
            ri = new ReflectInsight("TTSServicePOC.Service.Worker.TTSWorker");

            try
            {
                #region Load in the parameters from the Worker configuration

                //CacheSize = int.Parse(Parameters("cacheSize", "200"));

                #endregion

                synthesizer = new SpeechSynthesizer();                
                voices = SpeechSynthesizer.AllVoices;

                ri.SendInformation("Starting TTSWorker ({0})", this.Name);
            }
            catch (Exception ex)
            {
                ri.SendException(ex);
                throw;
            }
        }

        /// <summary>
        /// Called when [dispose].
        /// </summary>
        protected override void OnDispose()
        {
            base.OnDispose();

            ri.SendInformation("Stopping TTSWorker ({0})", this.Name);
        }

        /// <summary>
        /// Called when doing work.
        /// </summary>
        protected override async void OnWork()
        {
            try
            {
                ri.SendInformation("Executing TTSWorker ({0})", this.Name);

                await ProcessTTS();
            }
            catch (Exception ex)
            {
                ri.SendException("Unexpected Exception", ex);
            }
        }

        #endregion

        private async Task ProcessTTS()
        {
            using (TTSEntities context = new TTSEntities())
            {
                var itemsToProcess =  await (from u in context.TTSMessages
                                        where u.ProcessedDate.HasValue == false
                                        select u).ToListAsync<TTSMessage>();

                if (itemsToProcess.Count == 0)
                {
                    return;
                }

                ri.SendInformation("Found {0} items to process for speech to text", itemsToProcess.Count);            

                foreach (var item in itemsToProcess)
                {
                    ri.SendMessage("Record Id to process: '{0}'", item.TTSMessageId);

                    item.Filename = await CreateSpeechSynthesisFile(item);
                    item.ProcessedDate = DateTime.Now;

                    if (String.IsNullOrWhiteSpace(item.Filename))
                    {
                        continue;
                    }

                    await context.SaveChangesAsync();
                }                                       
            }
        }

        private async Task<string> CreateSpeechSynthesisFile(TTSMessage item)
        {
                string filename = String.Format("{0}.wav", Guid.NewGuid());

                try
                {
                    // get system voice that matches the gender
                    var voice = (from v in voices
                                 where v.Gender == (Windows.Media.SpeechSynthesis.VoiceGender)item.VoiceGender.VoiceGenderId
                                 select v).FirstOrDefault<VoiceInformation>();

                    synthesizer.Voice = voice;

                    // create the data stream
                    SpeechSynthesisStream synthesisStream;
                    try
                    {
                        synthesisStream = await synthesizer.SynthesizeTextToStreamAsync(item.Message);
                    }
                    catch (Exception ex)
                    {
                        ri.SendException("Unable to synthesize text", ex);
                        synthesisStream = null;
                        return string.Empty;
                    }

                    ri.SendMessage("Creating file '{0}'", filename);

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync("C:\\temp\\TTS"); //Windows.Storage.KnownFolders.DocumentsLibrary;// Windows.Storage.ApplicationData.Current.LocalFolder;
                    var option = CreationCollisionOption.ReplaceExisting;
                    StorageFile file = await folder.CreateFileAsync(filename, option);

                    ri.SendMessage("File '{0}' created. Text to speech synthesis has started...", file.Path); 

                    // open the output stream                    
                    Windows.Storage.Streams.Buffer buffer = new Windows.Storage.Streams.Buffer(4096);
                    IRandomAccessStream writeStream = (IRandomAccessStream)await file.OpenAsync(FileAccessMode.ReadWrite);
                    IOutputStream outputStream = writeStream.GetOutputStreamAt(0);
                    DataWriter dataWriter = new DataWriter(outputStream);

                    // copy the stream data into the file                    
                    while (synthesisStream.Position < synthesisStream.Size)
                    {
                        await synthesisStream.ReadAsync(buffer, 4096, InputStreamOptions.None);
                        dataWriter.WriteBuffer(buffer);
                    }

                    // close the data file streams
                    dataWriter.StoreAsync().AsTask().Wait();
                    outputStream.FlushAsync().AsTask().Wait();

                    ri.SendMessage("Text to speech synthesis completed for file '{0}'", file.Name);
                }
                catch (Exception e)
                {
                    ri.SendException(e);
                    return String.Empty;

                }
                return filename;
        }
    }
}
