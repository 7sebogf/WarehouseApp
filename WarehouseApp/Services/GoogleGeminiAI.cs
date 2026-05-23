using System;
using System.Threading.Tasks;

namespace WarehouseApp.Services
{
    public static class GoogleGeminiAI
    {
        public static async Task<string> SendMessageAsync(string userMessage)
        {
            await Task.Delay(300);
            return GetTestResponse(userMessage);
        }

        private static string GetTestResponse(string userMessage)
        {
            var msg = userMessage.ToLower();

            if (msg.Contains("анализ") || msg.Contains("склад") || msg.Contains("статистик"))
            {
                return "📊 **АНАЛИЗ СКЛАДА**\n\n" +
                       "📦 Всего товаров: 354 шт\n" +
                       "⚠️ Заканчиваются: 3 товара\n" +
                       "❌ Нет в наличии: 3 товара\n" +
                       "💰 Общая стоимость: 3 589 998,79 ₽\n\n" +
                       "✅ **Рекомендации:**\n• Пополнить запасы смартфонов\n• Заказать ноутбуки\n• Увеличить закупку холодильников";
            }

            if (msg.Contains("пополнение") || msg.Contains("заказать") || msg.Contains("сколько"))
            {
                return "🛒 **РЕКОМЕНДАЦИЯ ПО ПОПОЛНЕНИЮ**\n\n" +
                       "• Смартфоны: +10 шт\n• Ноутбуки: +8 шт\n• Холодильники: +5 шт\n• Пылесосы: +7 шт\n\n" +
                       "📦 **Общая сумма заказа:** ≈ 2 500 000 ₽";
            }

            if (msg.Contains("аналог") || msg.Contains("заменить") || msg.Contains("альтернатив"))
            {
                return "🔍 **ПОИСК АНАЛОГОВ**\n\n" +
                       "📱 **Смартфон:**\n   1. Samsung Galaxy\n   2. Xiaomi\n   3. Google Pixel\n\n" +
                       "💻 **Ноутбук:**\n   1. Dell XPS\n   2. HP Envy\n   3. Asus Zenbook";
            }

            if (msg.Contains("прогноз") || msg.Contains("продаж") || msg.Contains("тренд"))
            {
                return "📈 **ПРОГНОЗ ПРОДАЖ**\n\n" +
                       "📱 Смартфоны: +15% 📈\n💻 Ноутбуки: +10% 📈\n📺 Телевизоры: +5% 📈\n\n" +
                       "💡 **Совет:** Увеличьте закупки на 20%!";
            }

            if (msg.Contains("описание") || msg.Contains("характеристик"))
            {
                return "🏷️ **ОПИСАНИЕ ТОВАРА**\n\n" +
                       "✨ Отличный товар высокого качества!\n📱 Современный дизайн\n🔋 Долгое время работы\n🎨 Доступные цвета\n\n🔥 **В наличии!**";
            }

            return "🤖 **Я AI-помощник склада!**\n\n" +
                   "Вот что я умею:\n" +
                   "• 📊 Анализ склада\n• 🛒 Рекомендация пополнения\n• 🔍 Поиск аналогов\n• 📈 Прогноз продаж\n• 🏷️ Описание товара\n\n" +
                   "Что вас интересует?";
        }
    }
}