using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class Program
{
    public const string APIWEB = "https://api.dicebear.com/8.x";
    public static readonly string[] TYPES = new string[]
    {
        "/fun-emoji",
        "/avataaars",
        "/bottts",
        "/pixel-art",
    };


    private static void Main(string[] args)
    {
        var botClient = new TelegramBotClient("7096993951:AAF--b3vSpleRvigHfbsdGll1VyLjsTAb38");

        using var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        botClient.StartReceiving(
             updateHandler: HandleUpdateAsync,
             errorHandler: HandlePollingErrorAsync,
             receiverOptions: receiverOptions,
             cancellationToken: cts.Token
        );

        Console.ReadLine();

        async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            var message = new Message();
            if (update.Message != null)
                message = update.Message;

            if (update.Message!.Type != MessageType.Text)
            {
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Iltimos, matnli xabar yuboring.",
                    cancellationToken: token);
                return;
            }

            var messageText = update.Message.Text;
            var messageType = string.Empty;
            var messageArray = messageText.Split(' ');

            int spaceIndex = messageText.IndexOf(' ');
            if (spaceIndex != -1)
            {
                messageType = messageText[..spaceIndex].Trim();
                messageText = messageText[(spaceIndex + 1)..].Trim();
            }

            if (messageArray.Length < 2)
            {
                var command = messageArray[0].Trim();

                if (command == "/help" || command == "/start")
                {
                    Console.WriteLine($"chatId: {message.Chat.Id}, command: {command}, Status: Starting or helping!");
                    messageText = "Bot ishlash tartibi Avval buyruq keyin esa bo'sh joy tashlab matn kiritiladi va shu asosida sizga .png fayli qaytariladi.\n" +
                        "Buyqurlar: /fun-emoji, \t /avataaars, \t /bottts, \t /pixel-art";
                }
                else
                {
                    if (TYPES.Contains(messageArray[0].Trim()))
                    {
                        Console.WriteLine($"chatId: {message.Chat.Id}, command: {command}, seed:   , Status: Validation is not successed!");
                        messageText = "Iltimos, buyruqdan keyin matn (seed) kiriting.";
                    }
                    else
                    {
                        Console.WriteLine($"chatId: {message.Chat.Id}, command:  , seed: {messageText}, Status: Validation is not successed!");
                        messageText = "Iltimos, avatar olish uchun buyruqdan foydalaning.";
                    }
                }

                await bot.SendMessage(
                chatId: message.Chat.Id,
                text: messageText,
                cancellationToken: token);

                return;
            }

            messageType = messageArray[0].ToLowerInvariant();

            if (!TYPES.Contains(messageType))
            {
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Noma’lum buyruq...",
                    cancellationToken: token);
                Console.WriteLine($"chatId: {message.Chat.Id}, command: {messageType}, seed: {messageText}, Status: Command is not supported!");
                return;
            }

            var httpResponse = await GetEmojiAsync(messageType, messageText);

            if (!httpResponse.IsSuccessStatusCode)
            {
                messageText = "Avatar yaratishda xatolik yuz berdi. Keyinroq urinib ko‘ring.";
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: messageText,
                    cancellationToken: token);
                Console.WriteLine($"chatId: {message.Chat.Id}, command: {messageType}, seed: {message.Text}, Status: Get Avatar Stream is not success!");
                return;
            }

            await using var stream = await httpResponse.Content.ReadAsStreamAsync();
            stream.Position = 0;

            var res = await bot.SendPhoto(
                chatId: message.Chat.Id,
                new InputFileStream(stream, "emoji.png"),
                "Mana sizning emoji 😊",
                cancellationToken: token
            );

            Console.WriteLine($"chatId: {message.Chat.Id}, command: {messageType}, seed: {messageText}, Status: Successed!");
            return;
        }

        Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            Console.WriteLine(exception);
            return Task.CompletedTask;
        }
    }

    static async Task<HttpResponseMessage> GetEmojiAsync(string pngType, string seed)
    {
        using (var httpClient = new HttpClient())
        {
            var url = $"{APIWEB}{pngType}/png?seed={seed}";
            var response = await httpClient.GetAsync(url);

            return response;
        }
    }
}