using Orleans;
using Orleans.Streams;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Server.ViewModels
{
    // Currently not used, since could not figure out how to call StateHasChanged in View from OnNext method
    public class ListenerVM : ReactiveObject
    {
        private readonly IClusterClient client;

        [Reactive]
        public string Message { get; set; }

        [ObservableAsProperty]
        public string Incoming { get; }

        public ListenerVM(IClusterClient client)
        {
            this.client = client;


            var guid = new Guid("0f07450c-b8e7-472c-a275-0059bca40989");
            // Get stream provider
            var streamProvider = client.GetStreamProvider("SMSProvider");
            // Get the reference to a stream
            var stream = streamProvider.GetStream<string>(guid, "Razor");

            stream.SubscribeAsync(OnNext);

            this.WhenAnyValue(x => x.Message)
                .ToPropertyEx(this, m => m.Incoming);
        }
        

        Task OnNext(string message, StreamSequenceToken token)
        {
            Message = message;
            return Task.CompletedTask;
        }
    }
}
