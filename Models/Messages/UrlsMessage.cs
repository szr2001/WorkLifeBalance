using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WorkLifeBalance.Models.Messages;

public class UrlsMessage : ValueChangedMessage<string>
{
    public UrlsMessage(string value) : base(value)
    {
    }
}