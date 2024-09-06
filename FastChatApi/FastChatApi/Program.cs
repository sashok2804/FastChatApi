using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		// Добавление необходимых сервисов
		builder.Services.AddAuthorization();
		builder.Services.AddHttpClient<ChatHub>();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSignalR();


		// Настройка CORS
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowLocalhost",
				builder =>
				{
					builder.WithOrigins("http://localhost:5238") // Добавьте необходимые домены
						   .AllowAnyHeader()
						   .AllowAnyMethod()
						   .AllowCredentials(); // Разрешение использования куки и авторизации
				});
		});

		var app = builder.Build();



		// Включение маршрутизации
		app.UseRouting();

		// Использование CORS
		app.UseCors("AllowLocalhost");

		app.UseAuthorization();

		// Определение маршрутов
		app.MapHub<ChatHub>("/chathub");

		// Запуск приложения
		app.Run();
	}
}
