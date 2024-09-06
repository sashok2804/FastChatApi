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

		// ���������� ����������� ��������
		builder.Services.AddAuthorization();
		builder.Services.AddHttpClient<ChatHub>();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSignalR();


		// ��������� CORS
		builder.Services.AddCors(options =>
		{
			options.AddPolicy("AllowLocalhost",
				builder =>
				{
					builder.WithOrigins("http://localhost:5238") // �������� ����������� ������
						   .AllowAnyHeader()
						   .AllowAnyMethod()
						   .AllowCredentials(); // ���������� ������������� ���� � �����������
				});
		});

		var app = builder.Build();



		// ��������� �������������
		app.UseRouting();

		// ������������� CORS
		app.UseCors("AllowLocalhost");

		app.UseAuthorization();

		// ����������� ���������
		app.MapHub<ChatHub>("/chathub");

		// ������ ����������
		app.Run();
	}
}
