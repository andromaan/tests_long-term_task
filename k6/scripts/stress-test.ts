/**
 * Stress тест — стрес-тестування одночасних запитів (inquiries) до об'єктів.
 *
 * Згідно з ТЗ: стрес-тестування одночасних запитів (наприклад, багато покупців
 * намагаються відправити форму цікавості на один або кілька популярних об'єктів).
 *
 * Запуск: k6 run scripts/stress-test.ts
 */

import { check, sleep } from "k6";
import { Options } from "k6/options";
import { THRESHOLDS } from "../helpers/config.ts";
import { submitInquiry, getProperties } from "../helpers/api-client.ts";

export const options: Options = {
  stages: [
    { duration: "30s", target: 20 }, // Розгін до 20 VUs
    { duration: "1m", target: 50 }, // Наближення до стресу
    { duration: "2m", target: 150 }, // Точка відмови (спам багато запитів за короткий час)
    { duration: "1m", target: 150 }, // Утримання
    { duration: "30s", target: 0 }, // Швидке відновлення
  ],
  thresholds: {
    ...THRESHOLDS,
    // Збільшені допуски для стрес-тесту під екстремальним навантаженням
    http_req_duration: ["p(95)<1500", "p(99)<3000"],
  },
};

// Зберігатимемо валідні ID об'єктів, щоб не додавати запити до неіснуючих
let availablePropertyIds: number[] = [];

export default function () {
  // На першій ітерації VU знаходить можливі property IDs (Status 0 = Available)
  if (availablePropertyIds.length === 0) {
    const res = getProperties(); // Вибирає першу сторінку або без фільтру
    if (res.status === 200) {
      const props = JSON.parse(res.body as string) as any[];
      // Фільтруємо тілки доступні (щоб не отримати 400 BadRequest)
      availablePropertyIds = props
        .filter((p) => p.status === 0)
        .map((p) => p.id);
    }
    // Запасний варіант, якщо база порожня (побачимо 400 помилки)
    if (availablePropertyIds.length === 0) {
      availablePropertyIds = [1, 2, 3];
    }
  }

  // Вибираємо випадковий ID з доступних
  const propIndex = Math.floor(Math.random() * availablePropertyIds.length);
  const propertyId = availablePropertyIds[propIndex];

  // Генерація Payload
  const payload = {
    name: `Stress Tester ${__VU}-${__ITER}`,
    email: `buyer_${__VU}_${__ITER}@example.com`,
    phone: `+380${Math.floor(100000000 + Math.random() * 900000000)}`,
    message:
      "Дуже зацікавив ваш об'єкт нерухомості! Готовий приїхати на огляд вже завтра.",
  };

  // Робимо запит
  const response = submitInquiry(propertyId, payload);

  check(response, {
    "POST /api/properties/{id}/inquiries status is 201": (r) =>
      r.status === 201,
  });

  // Затримка менша для створення вищого стресового навантаження
  sleep(0.5);
}
