using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text;

public class ChatHub : Hub
{
	private readonly HttpClient _httpClient;

	public ChatHub(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public async Task SendMessage(int roomId, int userId, string message)
	{
		Console.WriteLine($"Attempting to send message to room {roomId}: {message}");

		try
		{
			// Проверка существования комнаты
			Console.WriteLine($"Sending message to group {roomId.ToString()}");
			await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", userId, message);
			Console.WriteLine($"Message successfully sent to group {roomId}");

			// Создание объекта сообщения
			var messagePayload = new
			{
				id = 0, // Используйте реальный идентификатор, если он есть
				roomId = roomId,
				userId = userId,
				message = message,
				messageDate = DateTime.UtcNow
			};

			// Сериализация объекта сообщения в JSON
			var jsonContent = JsonSerializer.Serialize(messagePayload);
			Console.WriteLine($"Serialized message payload: {jsonContent}");

			var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

			// Отправка POST-запроса
			var response = await _httpClient.PostAsync("http://localhost:5238/api/Chat", content);
			response.EnsureSuccessStatusCode();

			Console.WriteLine($"Message successfully sent to API. Status code: {response.StatusCode}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error sending message: {ex}");
		}
	}


	public async Task JoinRoom(int roomId)
	{
		await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
		Console.WriteLine($"Connection {Context.ConnectionId} joined room {roomId}");
	}

	public async Task LeaveRoom(int roomId)
	{
		await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
		Console.WriteLine($"Connection {Context.ConnectionId} left room {roomId}");
	}
}
