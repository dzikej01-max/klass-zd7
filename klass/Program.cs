using System;
using System.Collections.Generic;

namespace задание7
{
	// Интерфейсы
	public interface ISendable
	{
		bool Send();
		bool IsSent { get; }
	}

	public interface ISchedulable
	{
		void Schedule(DateTime time);
		DateTime ScheduledTime { get; }
	}

	public interface IGroupable
	{
		void AddRecipient(string recipient);
		List<string> Recipients { get; }
	}

	public interface IPrioritized
	{
		int Priority { get; set; }
	}

	public interface ITemplatable
	{
		void SetTemplate(string template);
		string GetMessage();
	}

	// Абстрактный базовый класс
	public abstract class Notification : ISendable, ISchedulable, IPrioritized
	{
		public bool IsSent { get; protected set; }
		public DateTime ScheduledTime { get; protected set; }
		public int Priority { get; set; }
		protected string Content { get; set; }

		public virtual bool Send()
		{
			if (IsSent) return false;

			if (ScheduledTime > DateTime.Now)
			{
				Console.WriteLine($"Запланировано на {ScheduledTime}");
				return false;
			}

			IsSent = true;
			Console.WriteLine($"Отправлено: {GetType().Name} - {Content}");
			return true;
		}

		public void Schedule(DateTime time)
		{
			ScheduledTime = time;
			Console.WriteLine($"Уведомление запланировано на {time}");
		}

		public abstract string GetTypeName();
	}

	// Конкретные классы
	public class Email : Notification, IGroupable, ITemplatable
	{
		public List<string> Recipients { get; private set; } = new List<string>();
		private string _template;

		public Email(string content)
		{
			Content = content;
		}

		public void AddRecipient(string recipient)
		{
			Recipients.Add(recipient);
		}

		public void SetTemplate(string template)
		{
			_template = template;
			Content = _template.Replace("{body}", Content);
		}

		public string GetMessage() => Content;

		public override string GetTypeName() => "Email";
	}

	public class SMS : Notification, IPrioritized
	{
		public SMS(string phoneNumber, string message)
		{
			Content = $"[{phoneNumber}] {message}";
		}

		public override string GetTypeName() => "SMS";
	}

	public class PushNotification : Notification, ISchedulable, IPrioritized
	{
		public PushNotification(string deviceId, string message)
		{
			Content = $"[{deviceId}] {message}";
		}

		public override string GetTypeName() => "Push";
	}

	public class AppNotification : Notification, IGroupable, ITemplatable
	{
		public List<string> Recipients { get; private set; } = new List<string>();
		private string _template;

		public AppNotification(string message)
		{
			Content = message;
		}

		public void AddRecipient(string recipient)
		{
			Recipients.Add(recipient);
		}

		public void SetTemplate(string template)
		{
			_template = template;
			Content = _template.Replace("{title}", Content);
		}

		public string GetMessage() => Content;

		public override string GetTypeName() => "AppNotification";
	}

	public class TelegramMessage : Notification, IGroupable, ITemplatable
	{
		public List<string> Recipients { get; private set; } = new List<string>();
		private string _template;

		public TelegramMessage(string chatId, string message)
		{
			Content = $"[{chatId}] {message}";
		}

		public void AddRecipient(string recipient)
		{
			Recipients.Add(recipient);
		}
		public void SetTemplate(string template)
		{
			_template = template;
			Content = _template.Replace("{text}", Content);
		}

		public string GetMessage() => Content;

		public override string GetTypeName() => "Telegram";
	}

	// Демонстрация полиморфизма
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("=== Система уведомлений ===\n");

			// Создаем коллекцию уведомлений
			List<Notification> notifications = new List<Notification>();

			// Email с шаблоном и группой
			var email = new Email("Добро пожаловать!");
			email.SetTemplate("Здравствуйте! {body}");
			email.AddRecipient("user@example.com");
			email.AddRecipient("admin@example.com");
			email.Priority = 1;
			notifications.Add(email);

			// SMS
			var sms = new SMS("+79991234567", "Ваш код: 1234");
			sms.Priority = 3;
			notifications.Add(sms);

			// Push-уведомление с планированием
			var push = new PushNotification("device_001", "Новое сообщение");
			push.Schedule(DateTime.Now.AddSeconds(5));
			push.Priority = 2;
			notifications.Add(push);

			// Уведомление в приложении
			var appNotif = new AppNotification("Обновление системы");
			appNotif.SetTemplate("Уведомление: {title}");
			appNotif.AddRecipient("user_123");
			notifications.Add(appNotif);

			// Telegram
			var telegram = new TelegramMessage("@username", "Привет!");
			telegram.SetTemplate("*{text}*");
			telegram.AddRecipient("@username");
			notifications.Add(telegram);

			// Демонстрация полиморфизма
			Console.WriteLine("Отправка всех уведомлений:\n");

			foreach (var notification in notifications)
			{
				Console.WriteLine($"Тип: {notification.GetTypeName()}, Приоритет: {notification.Priority}");

				if (notification is IGroupable groupable && groupable.Recipients.Count > 0)
				{
					Console.WriteLine($"  Получатели: {string.Join(", ", groupable.Recipients)}");
				}

				if (notification is ITemplatable templatable)
				{
					Console.WriteLine($"  С шаблоном: {templatable.GetMessage()}");
				}

				notification.Send();
				Console.WriteLine();
			}

			// Демонстрация работы с приоритетами
			Console.WriteLine("\n=== Сортировка по приоритету ===");
			notifications.Sort((a, b) => a.Priority.CompareTo(b.Priority));

			foreach (var notif in notifications)
			{
				Console.WriteLine($"{notif.GetTypeName()} - Приоритет: {notif.Priority}");
			}

			Console.WriteLine("\nНажмите любую клавишу для выхода...");
			Console.ReadKey();
		}
	}
}
