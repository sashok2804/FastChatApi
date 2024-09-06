using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

public class ChatHub : Hub
{
	// Отправка сообщения в указанную комнату
	public async Task SendMessage(string room, string user, string message)
	{
		Console.WriteLine($"Attempting to send message to room {room}: {message}");

		try
		{
			await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
			Console.WriteLine($"Message successfully sent to room {room}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error sending message: {ex.Message}");
		}
	}


	// Присоединение к указанной комнате
	public async Task JoinRoom(string room)
	{
		// Проверка на наличие имени комнаты
		if (string.IsNullOrWhiteSpace(room))
		{
			throw new ArgumentException("Room name cannot be null or whitespace.", nameof(room));
		}

		// Присоединение к группе
		await Groups.AddToGroupAsync(Context.ConnectionId, room);

		// Логирование успешного присоединения
		Console.WriteLine($"Connection {Context.ConnectionId} joined room {room}");
	}

	// Покидание указанной комнаты
	public async Task LeaveRoom(string room)
	{
		// Проверка на наличие имени комнаты
		if (string.IsNullOrWhiteSpace(room))
		{
			throw new ArgumentException("Room name cannot be null or whitespace.", nameof(room));
		}

		// Покидание группы
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);

		// Логирование успешного выхода
		Console.WriteLine($"Connection {Context.ConnectionId} left room {room}");
	}
}
