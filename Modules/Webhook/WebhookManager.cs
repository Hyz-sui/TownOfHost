using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TownOfHost.Modules.Webhook;

public sealed class WebhookManager : IDisposable
{
    public static WebhookManager Instance { get; } = new();

    // see https://discord.com/developers/docs/resources/webhook#execute-webhook
    private HttpClient httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(4),
    };

    private static readonly ILogHandler logger = Logger.Handler(nameof(WebhookManager));
    private bool disposedValue;

    public void StartSend(WebhookMessageBuilder builder, Action<OnCompleteArgs> onComplete = default)
    {
        try
        {
            if (!TryReadUrl(out var url))
            {
                logger.Warn("URL設定が正しくありません");
                onComplete?.Invoke(new(true, FailureReason.InvalidUrl));
                return;
            }
            var sendTask = SendAsync(builder, url, onComplete);
            sendTask.ContinueWith(task =>
            {
                if (task.Exception is { } aggregateException)
                {
                    logger.Warn("送信中に例外が発生しました");
                    logger.Exception(aggregateException.InnerException);
                }
            });
        }
        catch
        {
            onComplete?.Invoke(new(true, FailureReason.Exception));
            throw;
        }
    }
    private bool TryReadUrl(out string url)
    {
        if (CreateConfigFileIfNecessary())
        {
            url = null;
            return false;
        }
        using var stream = WebhookUrlFile.OpenRead();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var text = reader.ReadLine();
        if (ValidateUrl(text))
        {
            url = text;
            return true;
        }
        else
        {
            url = null;
            return false;
        }
    }
    public bool CreateConfigFileIfNecessary()
    {
        if (WebhookUrlFile.Exists)
        {
            return false;
        }
        using var stream = WebhookUrlFile.Create();
        using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.WriteLine("この文章をすべて削除してウェブフックのURLを入力し，上書き保存してください");
        return true;
    }
    private bool ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }
        return webhookUrlRegex.IsMatch(url);
    }
    public async Task SendAsync(WebhookMessageBuilder builder, string url, Action<OnCompleteArgs> onComplete = default, CancellationToken cancellationToken = default)
    {
        try
        {
            var fullMessage = builder.ContentBuilder.ToString();
            if (fullMessage.Length <= MaxContentLength)
            {
                if (await SendInnerAsync(fullMessage, builder.UserName, builder.AvatarUrl, url, cancellationToken))
                {
                    onComplete?.Invoke(new(false));
                }
                else
                {
                    onComplete?.Invoke(new(true, FailureReason.Network));
                }
                return;
            }

            var hasFailure = false;
            // 改行を区切りとして，上限文字数を超えないように分割して送信する
            // 1行で上限を超えているケースは考慮しない
            var lines = fullMessage.Split(Environment.NewLine);
            var partBuilder = new StringBuilder();
            foreach (var line in lines)
            {
                if (partBuilder.Length + line.Length > MaxContentLength)
                {
                    if (!await SendInnerAsync(partBuilder.ToString(), builder.UserName, builder.AvatarUrl, url, cancellationToken))
                    {
                        hasFailure = true;
                    }
                    partBuilder.Clear();
                    await Task.Delay(1000, cancellationToken);
                }
                partBuilder.AppendLine(line);
            }
            if (partBuilder.Length > 0)
            {
                if (!await SendInnerAsync(partBuilder.ToString(), builder.UserName, builder.AvatarUrl, url, cancellationToken))
                {
                    hasFailure = true;
                }
            }

            if (hasFailure)
            {
                onComplete?.Invoke(new(true, FailureReason.Network));
            }
            else
            {
                onComplete?.Invoke(new(false));
            }
        }
        catch
        {
            onComplete?.Invoke(new(true, FailureReason.Exception));
            throw;
        }
    }
    private async Task<bool> SendInnerAsync(string message, string userName, string avatarUrl, string url, CancellationToken cancellationToken = default)
    {
        var content = new WebhookRequest(message, userName, avatarUrl);
        return await SendAsync(content, url, cancellationToken);
    }
    private async Task<bool> SendAsync(WebhookRequest webhookRequest, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(url, webhookRequest, cancellationToken);
            logger.Info($"{(int)response.StatusCode} {response.ReasonPhrase}");
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn("送信に失敗");
                return false;
            }
            return true;
        }
        catch (TaskCanceledException taskCanceledException)
        {
            logger.Warn("送信はキャンセルされました");
            logger.Exception(taskCanceledException);
            return false;
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                httpClient.Dispose();
            }
            disposedValue = true;
        }
    }
    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public readonly struct OnCompleteArgs(bool hasFailure, FailureReason failureReason = default)
    {
        public bool HasFailure { get; } = hasFailure;
        public FailureReason FailureReason { get; } = failureReason;
    }
    public readonly struct FailureReason(string message)
    {
        public string Message { get; } = message;

        public static FailureReason InvalidUrl { get; } = new("URL設定が正しくありません。設定を確認してください");
        public static FailureReason Exception { get; } = new("処理中にエラーが発生しました");
        public static FailureReason Network { get; } = new("送信を完了できませんでした。\nネットワーク品質、Modの不具合、Discord側の問題が原因の可能性があります");

    }

    public FileInfo WebhookUrlFile { get; } =
#if DEBUG
        new("DebugWebhookUrl.txt");
#else
        new("WebhookUrl.txt");
#endif
    private readonly Regex webhookUrlRegex = new("^(https://(ptb.|canary.)?discord(app)?.com/api/webhooks/)");
    // 上限2,000文字．文字数カウントの実装が違うかもしれないけどよくわからないので余裕をもたせる
    private const int MaxContentLength = 1950;
}
