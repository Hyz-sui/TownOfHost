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

    public void StartSend(WebhookMessageBuilder builder)
    {
        if (!TryReadUrl(out var url))
        {
            logger.Warn("URL設定が正しくありません");
            return;
        }
        var sendTask = SendAsync(builder, url);
        sendTask.ContinueWith(task =>
        {
            if (task.Exception is { } aggregateException)
            {
                logger.Warn("送信中に例外が発生しました");
                logger.Exception(aggregateException.InnerException);
            }
        });
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
    public async Task SendAsync(WebhookMessageBuilder builder, string url, CancellationToken cancellationToken = default)
    {
        var fullMessage = builder.ContentBuilder.ToString();
        if (fullMessage.Length <= MaxContentLength)
        {
            await SendInnerAsync(fullMessage, builder.UserName, builder.AvatarUrl, url, cancellationToken);
            return;
        }
        // 改行を区切りとして，上限文字数を超えないように分割して送信する
        // 1行で上限を超えているケースは考慮しない
        var lines = fullMessage.Split(Environment.NewLine);
        var partBuilder = new StringBuilder();
        foreach (var line in lines)
        {
            if (partBuilder.Length + line.Length > MaxContentLength)
            {
                await SendInnerAsync(partBuilder.ToString(), builder.UserName, builder.AvatarUrl, url, cancellationToken);
                partBuilder.Clear();
                await Task.Delay(1000, cancellationToken);
            }
            partBuilder.AppendLine(line);
        }
        if (partBuilder.Length > 0)
        {
            await SendInnerAsync(partBuilder.ToString(), builder.UserName, builder.AvatarUrl, url, cancellationToken);
        }
    }
    private async Task SendInnerAsync(string message, string userName, string avatarUrl, string url, CancellationToken cancellationToken = default)
    {
        var content = new WebhookRequest(message, userName, avatarUrl);
        await SendAsync(content, url, cancellationToken);
    }
    private async Task SendAsync(WebhookRequest webhookRequest, string url, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(url, webhookRequest, cancellationToken);
            logger.Info($"{(int)response.StatusCode} {response.ReasonPhrase}");
            if (!response.IsSuccessStatusCode)
            {
                logger.Warn("送信に失敗");
            }
        }
        catch (TaskCanceledException taskCanceledException)
        {
            logger.Warn("送信はキャンセルされました");
            logger.Exception(taskCanceledException);
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
