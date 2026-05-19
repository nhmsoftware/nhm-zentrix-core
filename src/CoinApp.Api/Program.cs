using CoinApp.Api.Auth;
using CoinApp.Api.Localization;
using CoinApp.Api.Middleware;
using CoinApp.Application;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Options;
using CoinApp.Infrastructure;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApiLocalization();
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddApplication();
builder.Services.AddSingleton(new EmailVerificationOptions
{
    CodeExpirationMinutes = GetEmailVerificationCodeExpirationMinutes(builder.Configuration),
    ExposeCodeInResponse = GetEmailVerificationExposeCodeInResponse(builder.Configuration)
});
builder.Services.AddSingleton(new PasswordResetOptions
{
    CodeExpirationMinutes = GetPositiveInt(builder.Configuration, "PasswordReset:CodeExpirationMinutes", 15),
    ResetTokenExpirationMinutes = GetPositiveInt(builder.Configuration, "PasswordReset:ResetTokenExpirationMinutes", 15),
    MaxVerifyAttempts = GetPositiveInt(builder.Configuration, "PasswordReset:MaxVerifyAttempts", 5),
    ExposeCodeInResponse = GetBool(builder.Configuration, "PasswordReset:ExposeCodeInResponse")
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ICurrentUserContext, HttpCurrentUserContext>();
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.UseApiLocalization();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static int GetEmailVerificationCodeExpirationMinutes(IConfiguration configuration) =>
    GetPositiveInt(configuration, "EmailVerification:CodeExpirationMinutes", 15);

static bool GetEmailVerificationExposeCodeInResponse(IConfiguration configuration) =>
    GetBool(configuration, "EmailVerification:ExposeCodeInResponse");

static int GetPositiveInt(IConfiguration configuration, string key, int defaultValue) =>
    int.TryParse(configuration[key], out var value) && value > 0
        ? value
        : defaultValue;

static bool GetBool(IConfiguration configuration, string key) =>
    bool.TryParse(configuration[key], out var value) && value;

public partial class Program { }
