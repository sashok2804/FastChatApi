using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

public class ChatHub : Hub
{
	private readonly HttpClient _httpClient;

	public ChatHub(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	// Получение имени пользователя из токена
	private string GetUserNameFromToken(string token)
	{
		var handler = new JwtSecurityTokenHandler();
		var jwtToken = handler.ReadJwtToken(token);

		// Извлечение имени пользователя из токена 
		var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
		return nameClaim?.Value ?? "Unknown";
	}

	// Получение ID пользователя из токена
	private string GetUserIdFromToken(string token)
	{
		var handler = new JwtSecurityTokenHandler();
		var jwtToken = handler.ReadJwtToken(token);

		// Извлечение ID пользователя из токена 
		var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid);
		return idClaim?.Value ?? "Unknown";
	}


	// Получение JWT токена из куки
	private string GetTokenFromCookies()
	{
		if (Context.GetHttpContext().Request.Cookies.TryGetValue("Authorization", out var token))
		{
			return token.StartsWith("Bearer ") ? token.Substring(7) : token;
		}
		return null;
	}

	public async Task SendMessage(int roomId, string message, string token)
	{
		Console.WriteLine($"Attempting to send message to room {roomId}: {message}");

		try
		{
			// Удаление префикса "Bearer " из токена, если он присутствует
			if (token.StartsWith("Bearer "))
			{
				token = token.Substring(7);
			}

			// Если токен пустой, прекращаем выполнение
			if (string.IsNullOrEmpty(token))
			{
				Console.WriteLine("JWT Token not provided.");
				return;
			}

			// Извлекаем имя пользователя и ID пользователя из токена
			var userName = GetUserNameFromToken(token);
			var userId = GetUserIdFromToken(token);

			if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userId))
			{
				Console.WriteLine("User name or user ID could not be retrieved from JWT.");
				return;
			}

			// Отправка сообщения с именем пользователя
			Console.WriteLine($"Sending message to group {roomId.ToString()}");
			await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", userName, message);
			Console.WriteLine($"Message successfully sent to group {roomId}, {userName}");

			// Сериализация объекта сообщения для API
			var messagePayload = new
			{
				id = 0,
				roomId = roomId,
				userId = userId,
				message = message,
				messageDate = DateTime.UtcNow
			};

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
