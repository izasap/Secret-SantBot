using System;
using System.Data;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord;
using Discord.WebSocket;
using Ogettobot;

namespace Secret_SantBot
{
    class Program
    {
        DiscordSocketClient bot = new DiscordSocketClient();
        bool going = bool.Parse(File.ReadAllText(@"Need\going.txt"));
        ulong guild = ulong.Parse(File.ReadAllText(@"Need\guild.txt")),
            player = ulong.Parse(File.ReadAllText(@"Need\player.txt")),
            owner = ulong.Parse(File.ReadAllText(@"Need\owner.txt")),
            news = ulong.Parse(File.ReadAllText(@"Need\news.txt"));

        static void Main(string[] args)
            => new Program().Start().GetAwaiter().GetResult();

        async Task Start()
        {
            bot.Log += Log;
            bot.Ready += Ready;
            bot.ModalSubmitted += Modals;
            bot.ButtonExecuted += Buttons;

            await bot.LoginAsync(TokenType.Bot, File.ReadAllText(@"Need\token.txt"));
            await bot.StartAsync();

            Console.ReadLine();
        }

        Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        async Task Ready()
        {
            await bot.SetStatusAsync(UserStatus.Idle);
            /*var mainmenu = new ButtonBuilder()
                 .WithLabel("Главное меню")
                 .WithCustomId("mainmenu")
                 .WithStyle(ButtonStyle.Primary)
                 .WithEmote(Emoji.Parse("⭐"));
             var mc = new ComponentBuilder()
                 .WithButton(mainmenu);
             await bot.GetGuild(guild).GetTextChannel(ulong.Parse(File.ReadAllText(@"Need\secretsant.txt"))).SendMessageAsync("Чтобы начать, нажмите на кнопку!", false, null, null, null, null, mc.Build());
            */
        }

        async Task Modals(SocketModal mod)
        {
            if (mod != null)
            {
                if (mod.Data.CustomId == "news")
                {
                    List<SocketMessageComponentData> component = new(mod.Data.Components);
                    int count = int.Parse(File.ReadAllText($@"News\Count.txt")) + 1;

                    File.Create($@"News\{count}.txt").Close();
                    File.Create($@"NewsTitle\{count}.txt").Close();

                    File.WriteAllText($@"News\{count}.txt", component[1].Value);
                    File.WriteAllText($@"NewsTitle\{count}.txt", component[0].Value);
                    File.WriteAllText(@"News\Count.txt", count.ToString());
                    File.WriteAllText(@"NewsTitle\Count.txt", count.ToString());

                    await bot.GetGuild(guild).GetTextChannel(this.news).SendMessageAsync($"||<@&{this.player}>||\nСнег всё ещё идёт, а северные ветра принесли нам новые новости, проверте ваши почтовые ящики!");
                    await mod.RespondAsync("Новость записсана, участники уведомленны!", null, false, true);
                }

                else if (mod.Data.CustomId == "register")
                {
                    List<SocketMessageComponentData> component = new(mod.Data.Components);

                    File.Create($@"Users\{mod.User.Id.ToString()}.txt").Close();
                    File.WriteAllText($@"Users\{mod.User.Id}.txt", component[0].Value + "\n" + component[1].Value + "\nnull\nnull");

                    if (File.ReadAllText(@"Users\allplayers.txt").Length > 0)
                        File.WriteAllText(@"Users\allplayers.txt", File.ReadAllText($@"Users\allplayers.txt") + "\n" + mod.User.Id.ToString());
                    else
                        File.WriteAllText(@"Users\allplayers.txt", mod.User.Id.ToString());

                    await bot.GetGuild(guild).GetUser(mod.User.Id).AddRoleAsync(this.player);

                    var mainmenu = new ButtonBuilder()
                        .WithLabel("В главное меню")
                        .WithCustomId("mainmenu")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("⭐"));
                    var mc = new ComponentBuilder()
                        .WithButton(mainmenu);

                    await mod.RespondAsync("Готово, вы зарегестрировались", null, false, true, null, mc.Build());
                }

                else if (mod.Data.CustomId == "rename")
                {
                    List<SocketMessageComponentData> component = new(mod.Data.Components);
                    string[] stats = File.ReadAllLines($@"Users\{mod.User.Id.ToString()}.txt");
                    stats[0] = component[0].Value;
                    File.WriteAllLines($@"Users\{mod.User.Id.ToString()}.txt", stats);

                    await mod.RespondAsync("Готово, ваше ФИО изменено!", null, false, true);
                }

                else if (mod.Data.CustomId == "refavorite")
                {
                    List<SocketMessageComponentData> component = new(mod.Data.Components);
                    string[] stats = File.ReadAllLines($@"Users\{mod.User.Id.ToString()}.txt");
                    stats[1] = component[0].Value;
                    File.WriteAllLines($@"Users\{mod.User.Id.ToString()}.txt", stats);

                    await mod.RespondAsync("Готово, ваши пожелания к подарку изменены!", null, false, true);
                }

                else if (mod.Data.CustomId == "rephoto")
                {
                    List<SocketMessageComponentData> component = new(mod.Data.Components);
                    string[] stats = File.ReadAllLines($@"Users\{mod.User.Id.ToString()}.txt");
                    stats[2] = $@"{component[0].Value}";
                    File.WriteAllLines($@"Users\{mod.User.Id.ToString()}.txt", stats);

                    var photo = new EmbedBuilder()
                        .WithTitle("**Ваша фотография для профиля**")
                        .WithDescription("Убедитесь, что на фотографии вас хорошо видно!")
                        .WithImageUrl(stats[2])
                        .WithColor(new(45, 186, 193));

                    await mod.RespondAsync("", null, false, true, null, null, photo.Build());
                }
            }
        }

        async Task Buttons(SocketMessageComponent btn)
        {
            if (btn != null)
            {
                if (btn.Data.CustomId == "mainmenu")
                {
                    List<SocketRole> roles = new(bot.GetGuild(guild).GetUser(btn.User.Id).Roles);
                    bool registered = false;

                    foreach (SocketRole role in roles)
                        if (role.Id == this.owner || role.Id == this.player)
                            registered = true;

                    if (registered)
                    {
                        bool owner = false;

                        foreach (SocketRole role in roles)
                            if (role.Id == this.owner)
                                owner = true;

                        var mail = new ButtonBuilder()
                            .WithLabel("Проверить почтовый ящик")
                            .WithCustomId("mail")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("📬"));
                        var profile = new ButtonBuilder()
                            .WithLabel("Обо мне любимом")
                            .WithCustomId("profile")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("🚹"));
                        var settings = new ButtonBuilder()
                            .WithLabel("Управление")
                            .WithCustomId("settings")
                            .WithStyle(ButtonStyle.Secondary)
                            .WithEmote(Emoji.Parse("⚙️"));
                        var info = new ButtonBuilder()
                            .WithLabel("Информация")
                            .WithCustomId("info")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("ℹ️"));
                        var mc = new ComponentBuilder()
                            .WithButton(mail)
                            .WithButton(profile);

                        var main = new EmbedBuilder()
                            .WithTitle("**Главное меню**")
                            .WithDescription("Здесь проходит мероприятие 'Тайный санта', надеюсь на тебя!")
                            .WithColor(new(45, 186, 193));

                        if (owner)
                            mc.WithButton(settings);

                        mc.WithButton(info);

                        await btn.RespondAsync("", null, false, true, null, mc.Build(), main.Build());
                    }

                    else
                    {
                        var register = new ButtonBuilder()
                            .WithLabel("Зарегистрироваться")
                            .WithCustomId("register")
                            .WithStyle(ButtonStyle.Success)
                            .WithEmote(Emoji.Parse("⛩️"));
                        var mc = new ComponentBuilder()
                            .WithButton(register);
                        await btn.RespondAsync("Здравствуйте, добро пожаловать в 'Тайного санту', чтобы начать, зарегестрируйтесь", null, false, true, null, mc.Build());
                    }
                }

                else if (btn.Data.CustomId == "register")
                {
                    List<SocketRole> roles = new(bot.GetGuild(guild).GetUser(btn.User.Id).Roles);
                    bool registered = false;

                    foreach (SocketRole role in roles)
                        if (role.Id == this.player)
                            registered = true;

                    if (!registered)
                    {
                        var name = new TextInputBuilder()
                            .WithLabel("ФИО")
                            .WithCustomId("name");
                        var favorite = new TextInputBuilder()
                            .WithLabel("Какие у вас пожелания?")
                            .WithCustomId("favorite")
                            .WithPlaceholder("Здесь вы должны написать, что бы вы хотели получить, в качестве подарка");
                        var register = new ModalBuilder()
                            .WithTitle("Регистрация")
                            .WithCustomId("register")
                            .AddTextInput(name)
                            .AddTextInput(favorite);

                        await btn.RespondWithModalAsync(register.Build());
                    }

                    else
                        await btn.RespondAsync("Вы уже зарегистрированы!", null, false, true);
                }

                else if (btn.Data.CustomId == "mail")
                {
                    int count = int.Parse(File.ReadAllText($@"News\Count.txt"));

                    if (count > 0)
                    {
                        string description = $"1️⃣ - {File.ReadAllText($@"NewsTitle\{count}.txt")}";
                        File.WriteAllText(@"News\news1.txt", File.ReadAllText($@"News\{count}.txt"));
                        File.WriteAllText(@"NewsTitle\news1.txt", File.ReadAllText($@"NewsTitle\{count}.txt"));

                        File.WriteAllText(@"News\news2.txt", "");
                        File.WriteAllText(@"NewsTitle\news2.txt", "");

                        File.WriteAllText(@"News\news3.txt", "");
                        File.WriteAllText(@"NewsTitle\news3.txt", "");

                        File.WriteAllText(@"News\news4.txt", "");
                        File.WriteAllText(@"NewsTitle\news4.txt", "");

                        File.WriteAllText(@"News\news5.txt", "");
                        File.WriteAllText(@"NewsTitle\news5.txt", "");

                        File.WriteAllText(@"News\news6.txt", "");
                        File.WriteAllText(@"NewsTitle\news6.txt", "");

                        File.WriteAllText(@"News\news7.txt", "");
                        File.WriteAllText(@"NewsTitle\news7.txt", "");

                        File.WriteAllText(@"News\news8.txt", "");
                        File.WriteAllText(@"NewsTitle\news8.txt", "");

                        File.WriteAllText(@"News\news9.txt", "");
                        File.WriteAllText(@"NewsTitle\news9.txt", "");

                        File.WriteAllText(@"News\news10.txt", "");
                        File.WriteAllText(@"NewsTitle\news10.txt", "");

                        var firstnews = new ButtonBuilder()
                            .WithCustomId("news1")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("1️⃣"));

                        var secondnews = new ButtonBuilder()
                            .WithCustomId("news2")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("2️⃣"));

                        var thridnews = new ButtonBuilder()
                            .WithCustomId("news3")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("3️⃣"));

                        var fourthnews = new ButtonBuilder()
                            .WithCustomId("news4")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("4️⃣"));

                        var fivthnews = new ButtonBuilder()
                            .WithCustomId("news5")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("5️⃣"));

                        var sixthnews = new ButtonBuilder()
                            .WithCustomId("news6")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("6️⃣"));

                        var seventhnews = new ButtonBuilder()
                            .WithCustomId("news7")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("7️⃣"));

                        var eigрthnews = new ButtonBuilder()
                            .WithCustomId("news8")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("8️⃣"));

                        var ninthnews = new ButtonBuilder()
                            .WithCustomId("news9")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("9️⃣"));

                        var tenthnews = new ButtonBuilder()
                            .WithCustomId("news10")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("🔟"));

                        var mc = new ComponentBuilder()
                            .WithButton(firstnews);

                        if (count > 1)
                        {
                            description += $"\n2️⃣ - {File.ReadAllText($@"NewsTitle\{count - 1}.txt")}";
                            File.WriteAllText(@"News\news2.txt", File.ReadAllText($@"News\{count - 1}.txt"));
                            File.WriteAllText(@"NewsTitle\news2.txt", File.ReadAllText($@"NewsTitle\{count - 1}.txt"));
                            mc.WithButton(secondnews);
                        }

                        if (count > 2)
                        {
                            description += $"\n3️⃣ - {File.ReadAllText($@"NewsTitle\{count - 2}.txt")}";
                            File.WriteAllText(@"News\news3.txt", File.ReadAllText($@"News\{count - 2}.txt"));
                            File.WriteAllText(@"NewsTitle\news3.txt", File.ReadAllText($@"NewsTitle\{count - 2}.txt"));
                            mc.WithButton(thridnews);
                        }

                        if (count > 3)
                        {
                            description += $"\n4️⃣ - {File.ReadAllText($@"NewsTitle\{count - 3}.txt")}";
                            File.WriteAllText(@"News\news4.txt", File.ReadAllText($@"News\{count - 3}.txt"));
                            File.WriteAllText(@"NewsTitle\news4.txt", File.ReadAllText($@"NewsTitle\{count - 3}.txt"));
                            mc.WithButton(fourthnews);
                        }

                        if (count > 4)
                        {
                            description += $"\n5️⃣ - {File.ReadAllText($@"NewsTitle\{count - 4}.txt")}";
                            File.WriteAllText(@"News\news5.txt", File.ReadAllText($@"News\{count - 4}.txt"));
                            File.WriteAllText(@"NewsTitle\news5.txt", File.ReadAllText($@"NewsTitle\{count - 4}.txt"));
                            mc.WithButton(fivthnews);
                        }

                        if (count > 5)
                        {
                            description += $"\n6⃣ - {File.ReadAllText($@"NewsTitle\{count - 5}.txt")}";
                            File.WriteAllText(@"News\news6.txt", File.ReadAllText($@"News\{count - 5}.txt"));
                            File.WriteAllText(@"NewsTitle\news6.txt", File.ReadAllText($@"NewsTitle\{count - 5}.txt"));
                            mc.WithButton(sixthnews);
                        }

                        if (count > 6)
                        {
                            description += $"\n7⃣ - {File.ReadAllText($@"NewsTitle\{count - 6}.txt")}";
                            File.WriteAllText(@"News\news7.txt", File.ReadAllText($@"News\{count - 6}.txt"));
                            File.WriteAllText(@"NewsTitle\news7.txt", File.ReadAllText($@"NewsTitle\{count - 6}.txt"));
                            mc.WithButton(seventhnews);
                        }

                        if (count > 7)
                        {
                            description += $"\n8⃣ - {File.ReadAllText($@"NewsTitle\{count - 7}.txt")}";
                            File.WriteAllText(@"News\news8.txt", File.ReadAllText($@"News\{count - 7}.txt"));
                            File.WriteAllText(@"NewsTitle\news8.txt", File.ReadAllText($@"NewsTitle\{count - 7}.txt"));
                            mc.WithButton(eigрthnews);
                        }

                        if (count > 8)
                        {
                            description += $"\n9⃣ - {File.ReadAllText($@"NewsTitle\{count - 8}.txt")}";
                            File.WriteAllText(@"News\news9.txt", File.ReadAllText($@"News\{count - 8}.txt"));
                            File.WriteAllText(@"NewsTitle\news9.txt", File.ReadAllText($@"NewsTitle\{count - 8}.txt"));
                            mc.WithButton(ninthnews);
                        }

                        if (count > 9)
                        {
                            description += $"\n🔟 - {File.ReadAllText($@"NewsTitle\{count - 9}.txt")}";
                            File.WriteAllText(@"News\news10.txt", File.ReadAllText($@"News\{count - 9}.txt"));
                            File.WriteAllText(@"NewsTitle\news10.txt", File.ReadAllText($@"NewsTitle\{count - 9}.txt"));
                            mc.WithButton(tenthnews);
                        }

                        var news = new EmbedBuilder()
                            .WithTitle("**Почтовый ящик**")
                            .WithDescription(description)
                            .WithColor(new(45, 186, 193));

                        await btn.RespondAsync("", null, false, true, null, mc.Build(), news.Build());
                    }

                    else
                        await btn.RespondAsync("Новостей нет", null, false, true);
                }

                else if (btn.Data.CustomId == "news1" || btn.Data.CustomId == "news2" || btn.Data.CustomId == "news3" || btn.Data.CustomId == "news4" || btn.Data.CustomId == "news5" || btn.Data.CustomId == "news6" || btn.Data.CustomId == "news7" || btn.Data.CustomId == "news8" || btn.Data.CustomId == "news9" || btn.Data.CustomId == "news10")
                {
                    string place = btn.Data.CustomId;

                    var news = new EmbedBuilder()
                        .WithTitle($@"**{File.ReadAllText($@"NewsTitle\{btn.Data.CustomId}.txt")}**")
                        .WithDescription(File.ReadAllText($@"News\{btn.Data.CustomId}.txt"))
                        .WithColor(new(45, 186, 193));

                    var mainmenu = new ButtonBuilder()
                        .WithLabel("В главное меню")
                        .WithCustomId("mainmenu")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("⭐"));
                    var mail = new ButtonBuilder()
                        .WithLabel("Вернуться на почту")
                        .WithCustomId("mail")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("📬"));
                    var mc = new ComponentBuilder()
                        .WithButton(mainmenu)
                        .WithButton(mail);

                    await btn.RespondAsync("", null, false, true, null, mc.Build(), news.Build());
                }

                else if (btn.Data.CustomId == "profile")
                {
                    List<SocketRole> roles = new(bot.GetGuild(guild).GetUser(btn.User.Id).Roles);
                    bool player = false;

                    foreach (SocketRole role in roles)
                    {
                        if (role.Id == this.player)
                            player = true;
                    }

                    if (!player)
                    {
                        var register = new ButtonBuilder()
                            .WithLabel("Зарегистрироваться")
                            .WithCustomId("register")
                            .WithStyle(ButtonStyle.Success)
                            .WithEmote(Emoji.Parse("⛩️"));
                        var mc = new ComponentBuilder()
                            .WithButton(register);
                        await btn.RespondAsync("Эта категория предначначена для участников, зарегестрируйся, чтобы получить к ней доступ.", null, false, true, null, mc.Build());
                    }

                    else
                    {
                        var edit = new ButtonBuilder()
                            .WithLabel("Редактировать")
                            .WithCustomId("edit")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("📝"));
                        var client = new ButtonBuilder()
                            .WithLabel("Ваша цель")
                            .WithCustomId("client")
                            .WithStyle(ButtonStyle.Success)
                            .WithEmote(Emoji.Parse("🎁"));
                        var remove = new ButtonBuilder()
                            .WithLabel("Удалить")
                            .WithCustomId("remove")
                            .WithStyle(ButtonStyle.Danger)
                            .WithEmote(Emoji.Parse("🗑️"));

                        if (going)
                            remove.IsDisabled = true;

                        var mc = new ComponentBuilder()
                            .WithButton(edit)
                            .WithButton(client)
                            .WithButton(remove);

                        var stats = File.ReadAllLines($@"Users\{btn.User.Id}.txt");

                        var profile = new EmbedBuilder()
                            .WithTitle("**Ваш профиль**")
                            .WithDescription($"ФИО: {stats[0]}\n" +
                            $"Что нравится: {stats[1]}")
                            .WithColor(new(45, 186, 193));

                        if (stats[2] != "null")
                            profile.ImageUrl = stats[2];

                        await btn.RespondAsync("", null, false, true, null, mc.Build(), profile.Build());
                    }
                }

                else if (btn.Data.CustomId == "edit")
                {
                    var name = new ButtonBuilder()
                        .WithLabel("ФИО")
                        .WithCustomId("name")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("📝"));
                    var favorite = new ButtonBuilder()
                        .WithLabel("Пожелания")
                        .WithCustomId("favorite")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("📝"));
                    var photo = new ButtonBuilder()
                        .WithLabel("Фотография")
                        .WithCustomId("photo")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("📝"));
                    var mc = new ComponentBuilder()
                        .WithButton(name)
                        .WithButton(favorite)
                        .WithButton(photo);

                    await btn.RespondAsync("Что вы хотите изменить?", null, false, true, null, mc.Build());
                }

                else if (btn.Data.CustomId == "name")
                {
                    var name = new TextInputBuilder()
                        .WithLabel("ФИО")
                        .WithCustomId("name");
                    var rename = new ModalBuilder()
                        .WithTitle("Переименовать")
                        .WithCustomId("rename")
                        .AddTextInput(name);

                    await btn.RespondWithModalAsync(rename.Build());
                }

                else if (btn.Data.CustomId == "favorite")
                {
                    var favorite = new TextInputBuilder()
                        .WithLabel("Какие у вас пожелания?")
                        .WithCustomId("favorite")
                        .WithPlaceholder("Здесь вы должны написать, что бы вы хотели получить, в качестве подарка");
                    var refavorite = new ModalBuilder()
                        .WithTitle("Новое пожелание")
                        .WithCustomId("refavorite")
                        .AddTextInput(favorite);

                    await btn.RespondWithModalAsync(refavorite.Build());
                }

                else if (btn.Data.CustomId == "photo")
                {
                    var photo = new TextInputBuilder()
                        .WithLabel("Какие у вас пожелания?")
                        .WithCustomId("photo")
                        .WithPlaceholder("Здесь вы должны вставить ссылку на фотографию");
                    var rephoto = new ModalBuilder()
                        .WithTitle("Ваша фотография")
                        .WithCustomId("rephoto")
                        .AddTextInput(photo);

                    await btn.RespondWithModalAsync(rephoto.Build());
                }

                else if (btn.Data.CustomId == "client")
                {
                    if (going)
                    {
                        var mainmenu = new ButtonBuilder()
                            .WithLabel("В главное меню")
                            .WithCustomId("mainmenu")
                            .WithStyle(ButtonStyle.Primary)
                            .WithEmote(Emoji.Parse("⭐"));
                        var mc = new ComponentBuilder()
                            .WithButton(mainmenu);

                        string[] user = File.ReadAllLines($@"Users\{btn.User.Id}.txt");
                        var stats = File.ReadAllLines($@"Users\{user[3]}.txt");

                        var profile = new EmbedBuilder()
                            .WithTitle($"**Профиль {stats[0]}**")
                            .WithDescription($"ФИО: {stats[0]}\n" +
                            $"Что нравится: {stats[1]}")
                            .WithColor(new(45, 186, 193));

                        if (stats[2] != "null")
                        {
                            profile.ImageUrl = stats[2];
                        }

                        await btn.RespondAsync("", null, false, true, null, mc.Build(), profile.Build());
                    }

                    else
                        await btn.RespondAsync("Мероприятие ещё не началось!", null, false, true);
                }

                else if (btn.Data.CustomId == "remove")
                {
                    var yes = new ButtonBuilder()
                        .WithLabel("Да")
                        .WithCustomId("yes")
                        .WithStyle(ButtonStyle.Danger);
                    var mc = new ComponentBuilder()
                        .WithButton(yes);

                    await btn.RespondAsync("Вы действительно хотите удалить аккаунт?", null, false, true, null, mc.Build());
                }

                else if (btn.Data.CustomId == "yes")
                {
                    string[] users = File.ReadAllLines(@"Users\allplayers.txt");
                    List<string> newusers = new List<string>();

                    for (int i = 0; i < users.Length - 1; i++)
                        if (users[i] != btn.User.Id.ToString() && users[i] != "")
                            newusers.Add(users[i]);

                    File.WriteAllLines(@"Users\allplayers.txt", newusers);
                    File.Delete($@"Users\{btn.User.Id}.txt");

                    await bot.GetGuild(guild).GetUser(btn.User.Id).RemoveRoleAsync(this.player);

                    await btn.RespondAsync("Готово, вы больше не учавствуете в мероприяьие", null, false, true);
                }

                else if (btn.Data.CustomId == "settings")
                {
                    var message = new ButtonBuilder()
                        .WithLabel("Отправить новость")
                        .WithCustomId("message")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("📨"));
                    var players = new ButtonBuilder()
                        .WithLabel("Участники")
                        .WithCustomId("players")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("🚹"));
                    var start = new ButtonBuilder()
                        .WithLabel("Начать")
                        .WithCustomId("start")
                        .WithStyle(ButtonStyle.Success)
                        .WithEmote(Emoji.Parse("🎁"));
                    var end = new ButtonBuilder()
                        .WithLabel("Закончить")
                        .WithCustomId("end")
                        .WithStyle(ButtonStyle.Danger)
                        .WithEmote(Emoji.Parse("🏁"));

                    if (going)
                        start.WithDisabled(true);
                    else
                        end.WithDisabled(true);

                    var mc = new ComponentBuilder()
                        .WithButton(message)
                        .WithButton(players)
                        .WithButton(start)
                        .WithButton(end);

                    await btn.RespondAsync("", null, false, true, null, mc.Build());
                }

                if (btn.Data.CustomId == "message")
                {
                    var title = new TextInputBuilder()
                        .WithLabel("Название новости")
                        .WithCustomId("newstitle")
                        .WithPlaceholder("Здесь вы должны указать название (лозунг), которое будет отображаться небольшим текстом в новостях");
                    var text = new TextInputBuilder()
                        .WithLabel("Содержание")
                        .WithCustomId("newstext")
                        .WithPlaceholder("Здесь вы должны написать новость, которую получят люди");
                    var news = new ModalBuilder()
                        .WithTitle("О чём хотите уведомить участников?")
                        .WithCustomId("news")
                        .AddTextInput(title)
                        .AddTextInput(text);

                    await btn.RespondWithModalAsync(news.Build());
                }

                if (btn.Data.CustomId == "players")
                {
                    string[] allplayers = File.ReadAllLines(@"Users\allplayers.txt");
                    string message = "";

                    if (allplayers.Length > 0)
                    {
                        message += $"<@{allplayers[0]}> - {File.ReadAllLines($@"Users\{allplayers[0]}.txt")[0]}";
                        for (int i = 1; i < allplayers.Length; i++)
                            if (allplayers[i] != "")
                                message += $"\n<@{allplayers[i]}> - {File.ReadAllLines($@"Users\{allplayers[i]}.txt")[0]}";

                        var players = new EmbedBuilder()
                            .WithTitle("**Список участников**")
                            .WithDescription(message)
                            .WithColor(new(45, 186, 193));

                        await btn.RespondAsync("", null, false, true, null, null, players.Build());
                    }

                    else
                        await btn.RespondAsync("Участников нет!", null, false, true);
                }

                else if (btn.Data.CustomId == "start")
                {
                    bool can = false;
                    int count = 0;
                    string[] players = File.ReadAllLines(@"Users\allplayers.txt");

                    for (int i = 0; i < players.Length; i++)
                    {
                        if (players[i] != "")
                            count++;
                    }

                    if (count > 1)
                    {
                        can = true;
                    }

                    if (can)
                    {
                        Distribution dis = new();
                        dis.Distribut();

                        File.WriteAllText("going.txt", "True");
                        going = true;

                        var start = new EmbedBuilder()
                            .WithTitle("**Начало**")
                            .WithDescription("Мероприятие 'Тайный санта' началось, все вы уже получили участника, которого вы будете поздравлять, чтобы узнать кто это, зайдите в 'профиль', а после в 'цель', там вы сможете увидеть описание этого счастливчика.\nЖелаю удачи!")
                            .WithColor(new(0, 255, 0));

                        await bot.GetGuild(guild).GetTextChannel(this.news).SendMessageAsync($"||<@&{this.player}>||", false, start.Build());
                        await btn.RespondAsync("Мероприятие 'Тайный санта' началось!", null, false, true);
                    }

                    else
                        await btn.RespondAsync("Недостаточно участников для начала мероприятия 'Тайный санта'!", null, false, true);
                }

                else if (btn.Data.CustomId == "end")
                {
                    File.WriteAllText("going.txt", "False");
                    going = false;

                    var end = new EmbedBuilder()
                        .WithTitle("**Конец**")
                        .WithDescription("Мероприятие 'Тайный санта' закончилось, надеюсь, вы успели сделать всё необходимое и готовы подарить это другим участникам!")
                        .WithColor(new(255, 0, 0));

                    await bot.GetGuild(guild).GetTextChannel(this.news).SendMessageAsync($"||<@&{this.player}>||", false, end.Build());
                    await btn.RespondAsync("Мероприятие 'Тайный санта' закончилось!", null, false, true);
                }

                else if (btn.Data.CustomId == "info")
                {
                    var zahar = new ButtonBuilder()
                        .WithLabel("Разработчик")
                        .WithCustomId("zahar")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("⭐"));
                    var mc = new ComponentBuilder()
                        .WithButton(zahar);

                    var info = new EmbedBuilder()
                        .WithTitle("**Информация**")
                        .WithDescription("Secret SantBot - это бот, для игры в тайного санту. Каждому участнику предлагается осчастливить другого, подарив подарок на Новый год.")
                        .WithColor(new(45, 186, 193));

                    await btn.RespondAsync("", null, false, true, null, mc.Build(), info.Build());
                }

                else if (btn.Data.CustomId == "zahar")
                {
                    var mainmenu = new ButtonBuilder()
                        .WithLabel("В главное меню")
                        .WithCustomId("mainmenu")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("⭐"));
                    var info = new ButtonBuilder()
                        .WithLabel("К общей информации")
                        .WithCustomId("info")
                        .WithStyle(ButtonStyle.Primary)
                        .WithEmote(Emoji.Parse("ℹ️"));
                    var mc = new ComponentBuilder()
                        .WithButton(mainmenu)
                        .WithButton(info);

                    var infos = new EmbedBuilder()
                        .WithTitle("**Разработчик**")
                        .WithDescription("Кремененко Захар - <@766190130372542496>")
                        .WithImageUrl("https://cdn.discordapp.com/attachments/1038198767616274452/1046089878884061244/photo_2022-11-26_18-47-26.jpg")
                        .WithColor(new(47, 49, 54));

                    await btn.RespondAsync("", null, false, true, null, mc.Build(), infos.Build());
                }
            }
        }
    }
}