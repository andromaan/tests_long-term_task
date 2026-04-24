import { check, sleep } from "k6";
import { Options } from "k6/options";
import { THRESHOLDS } from "../helpers/config.ts";
import { getProperties } from "../helpers/api-client.ts";

export const options: Options = {
  stages: [
    { duration: "1m", target: 50 },
    { duration: "3m", target: 50 },
    { duration: "1m", target: 0 },
  ],
  thresholds: THRESHOLDS,
};

const CITIES = ["Kyiv", "Lviv", "Odesa", "Dnipro", "Kharkiv", "Rivne"];
const TYPES = [0, 1, 2];

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

  const queryParams = `city=${targetCity}&type=${targetType}&minPrice=${minPrice}&maxPrice=${maxPrice}&bedrooms=${bedrooms}`;

  const res = getProperties(queryParams);

  check(res, {
    "GET /api/properties?filters status is 200": (r) => r.status === 200,
    "Response is returned under 500ms": (r) => r.timings.duration < 500,
  });

  // Затримка 1-2 сек між запитами для реалістичності поведінки користувача
  sleep(getRandomInt(10, 20) / 10.0);
}
