/**
 * Load тест — перевірка пошуку об'єктів нерухомості з кількома фільтрами.
 *
 * Згідно з ТЗ: Навантажувальне тестування пошуку об'єктів з кількома фільтрами.
 * Мета: перевірити поведінку бази даних при складних запитах (фільтри за містом, типом, спальнями та ціною).
 *
 * Запуск: k6 run scripts/load-test.ts
 */

import { check, sleep } from "k6";
import { Options } from "k6/options";
import { THRESHOLDS } from "../helpers/config.ts";
import { getProperties } from "../helpers/api-client.ts";

export const options: Options = {
  stages: [
    { duration: "1m", target: 50 }, // Плавно підіймаємо до 50 VUs
    { duration: "3m", target: 50 }, // Стабільне навантаження (50 користувачів одночасно роблять важкі вибірки)
    { duration: "1m", target: 0 }, // Зниження навантаження
  ],
  thresholds: THRESHOLDS, // Очікуємо p(95) < 500ms
};

const CITIES = ["Kyiv", "Lviv", "Odesa", "Dnipro", "Kharkiv", "Rivne"];
const TYPES = [0, 1, 2]; // 0: Apartment, 1: House, 2: Commercial

function getRandomInt(min: number, max: number) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

export default function () {
  // Випадкові фільтри для імітації реальної поведінки користувачів
  const targetCity = CITIES[getRandomInt(0, CITIES.length - 1)];
  const targetType = TYPES[getRandomInt(0, TYPES.length - 1)];
  const minPrice = getRandomInt(40000, 100000);
  const maxPrice = minPrice + getRandomInt(50000, 300000);
  const bedrooms = getRandomInt(1, 4);

  // Формування параметрів запиту (багато умов WHERE у PostgreSQL)
  const queryParams = `city=${targetCity}&type=${targetType}&minPrice=${minPrice}&maxPrice=${maxPrice}&bedrooms=${bedrooms}`;

  // Виконання пошуку
  const res = getProperties(queryParams);

  check(res, {
    "GET /api/properties?filters status is 200": (r) => r.status === 200,
    "Response is returned under 500ms": (r) => r.timings.duration < 500,
  });

  // Затримка 1-2 сек між запитами для реалістичності поведінки користувача
  sleep(getRandomInt(10, 20) / 10.0);
}
