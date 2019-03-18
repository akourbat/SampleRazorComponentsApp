using Orleans;
using Orleans.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.Server.Grains
{
    public class Activity
    {
        public Guid Id { get; set; }

        public string Status { get; set; }

        public int MessagesCount { get; set; }
    }

    public class ActivityMessage
    {
        public int UserId { get; set; }

        public string Message { get; set; }

        public DateTime Created { get; set; }
    }

    public class LoanWorkFlowCommand
    {
        public Guid Id { get; set; }

        public int Amount { get; set; }

        public int Days { get; set; }

        public string Description { get; set; }
    }

    public interface ILoanWorkFlow: IGrainWithGuidKey
    {
        Task<Guid> RegisterLoanWorkFlow(LoanWorkFlowCommand command);

        Task ApproveAsync();

        Task PostMessage(ActivityMessage message);

        Task<LoanWorkFlowCommand> GetDetails();
    }

    public class LoanWorkFlow: Grain, ILoanWorkFlow
    {
        private const string channel = "d42fb0a0-9a84-41d8-9e94-6a4ae077f3f3";

        private LoanWorkFlowCommand data;

        private List<ActivityMessage> Messages = new List<ActivityMessage>();

        private string status;

        private IAsyncStream<Activity> activityStream;


        public override Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider("SMSProvider");
            var mainStreamId = new Guid(channel);
            activityStream = streamProvider.GetStream<Activity>(mainStreamId, "Treasury");

            return base.OnActivateAsync();
        }

        public async Task<Guid> RegisterLoanWorkFlow (LoanWorkFlowCommand command)
        {
            data = command;
            data.Id = this.GetPrimaryKey();
            status = "Created";
            var id = this.GetPrimaryKey();
            
            await activityStream.OnNextAsync(new Activity { Id = id, Status = status });
            return id;
        }

        public async Task ApproveAsync()
        {
            status = "Approved";
            await activityStream.OnNextAsync(new Activity { Id = this.GetPrimaryKey(), Status = status, MessagesCount = Messages.Count });
            return;
        }

        public async Task PostMessage(ActivityMessage message)
        {
            if (message != null && !string.IsNullOrEmpty(message.Message))
            {
                Messages.Add(message);
                await activityStream.OnNextAsync(new Activity { Id = this.GetPrimaryKey(), Status = status, MessagesCount = Messages.Count });
                return;
            }
        }

        public async Task<LoanWorkFlowCommand> GetDetails()
        {
            
            return data;
        }
    }
}
