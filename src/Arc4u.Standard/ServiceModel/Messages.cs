using System.Runtime.CompilerServices;
using Arc4u.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Arc4u.ServiceModel;

public class Messages : List<Message>
{
    private readonly bool _ignoreWarnings;

    public Messages(bool ignoreWarnings = false)
    {
        _ignoreWarnings = ignoreWarnings;
    }

    public IEnumerable<Message> Information
    {
        get { return this.Where(m => m.Type == MessageType.Information); }
    }

    public IEnumerable<Message> Warnings
    {
        get { return this.Where(m => m.Type == MessageType.Warning); }
    }

    public IEnumerable<Message> Errors
    {
        get { return this.Where(m => m.Type == MessageType.Error || m.Type == MessageType.Critical); }
    }

    public static Messages FromEnum(IEnumerable<Message> messages)
    {
        var newMessages = new Messages();
        newMessages.AddRange(messages);
        return newMessages;
    }

    private bool AlreadyAdded(Message item)
    {
        return this.Any(message => message.Text == item.Text && message.Category == item.Category);
    }

    public new void Add(Message message)
    {
        if (!AlreadyAdded(message))
        {
            base.Add(message);
        }
    }

    public bool WouldThrow()
    {
        return Errors.Any() || (Warnings.Any() && !_ignoreWarnings);
    }

    public void LogAndThrowIfNecessary<T>(ILogger<T> logger, [CallerMemberName] string methodName = "")
    {
        LogAll(logger, methodName);
        ThrowIfNecessary();
    }

    /// <summary>
    /// Localized and log all messages.
    /// </summary>
    public void LogAll<T>(ILogger<T> logger, [CallerMemberName] string methodName = "")
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(logger);
#else
        if (null == logger)
        {
            throw new ArgumentNullException(nameof(logger));
        }
#endif
        var _type = typeof(T);

        ForEach((m) =>
        {
            if (null == m?.Text)
            {
                return;
            }
            var lm = m.LocalizeMessage(Threading.Culture.Neutral);
            var categoryLogger = m.Category == MessageCategory.Business ? logger.Business(methodName) : logger.Technical(methodName);
            CommonLoggerProperties? property = null;

            property = m.Type switch
            {
                MessageType.Information => categoryLogger.Information(lm.Text ?? m.Text),
                MessageType.Warning => categoryLogger.Warning(lm.Text ?? m.Text),
                MessageType.Error => categoryLogger.Error(lm.Text ?? m.Text),
                MessageType.Critical => categoryLogger.Fatal(lm.Text ?? m.Text),
                _ => categoryLogger.Debug(lm.Text ?? m.Text),
            };

            if (!string.IsNullOrWhiteSpace(m.Code))
            {
                if (!string.IsNullOrWhiteSpace(m.Code))
                {
                    property.Add("Code", m.Code!);
                }
            }

            if (!string.IsNullOrWhiteSpace(m.Subject))
            {
                property.Add("Subject", m.Subject!);
            }

            property.Log();
        });
    }

    public void ThrowIfNecessary()
    {
        if (WouldThrow())
        {
            var translatedMessages = new Messages();
            ForEach(m => translatedMessages.Add(m.LocalizeMessage()));
            throw new AppException(translatedMessages);
        }
    }

    public void LocalizeAll()
    {
        for (var i = 0; i < this.Count; i++)
        {
            this[i] = this[i].LocalizeMessage();
        }
    }
}

