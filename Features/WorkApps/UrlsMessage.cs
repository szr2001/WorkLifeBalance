using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WorkLifeBalance.Features.WorkApps
{
    public class UrlsMessage : ValueChangedMessage<string>
    {
        public UrlsMessage(string value)
            : base(value)
        {
        }
    }
}
