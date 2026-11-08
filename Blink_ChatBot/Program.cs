using Blink_ChatBot;
using Blink_ChatBot.Components;
using Blink_ChatBot.Data;
using Blink_ChatBot.Services;
using Blink_ChatBot.Services.Gemini;
using Google.GenAI.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.FileIO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddChatClient(options =>
{
    var configuration = options.GetRequiredService<IConfiguration>();
    var provider = "gemini";
    var model = "gemini-3.5-flash-lite";

    var geminiKey = configuration["GEMINI_API_KEY"] ?? configuration["GEMINI_KEY"];
    if (string.IsNullOrWhiteSpace(geminiKey))
    {
        throw new InvalidOperationException("Gemini API key is missing. Set GEMINI_API_KEY or GEMINI_KEY in User Secrets (Development) or environment variables.");
    }

    var client = provider switch
    {
        "gemini" => new Google.GenAI.Client(apiKey: geminiKey).AsIChatClient(model),
        _ => throw new ArgumentException("Unsupported provider")
    };

    return client.AsBuilder()
        .UseFunctionInvocation(null, c =>
        {
            c.IncludeDetailedErrors = true;
        })
        .Build(options.GetRequiredService<IServiceProvider>());
});

builder.Services.AddTransient<ChatOptions>(sp => new ChatOptions {
    Tools = [.. Tools.GetTools(sp)]
});

builder.Services.AddHttpClient<IWeatherService, WeatherAPIService>();
builder.Services.AddTransient<EvaluateWeatherConditions>();

builder.Services.AddTransient<EmailGetService>();
builder.Services.AddTransient<EmailSendService>();


builder.Services.AddScoped<IChatbot, Gemini>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
