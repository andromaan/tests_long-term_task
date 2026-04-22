/**
 * Smoke тест — базова перевірка API.
 *
 * Згідно за прикладом. Перевірка мінімальної життєздатності
 * - пошуку нерухомості з базовими критеріями
 */

import { check, sleep } from "k6";
import { Options } from "k6/options";
import { THRESHOLDS } from "../helpers/config.ts";
import { getProperties } from "../helpers/api-client.ts";

export const options: Options = {
  vus: 1, // 1 віртуальний користувач
  duration: "20s", // протягом 20 секунд
  thresholds: THRESHOLDS,
};

export default function () {
  const queryParams = "city=Kyiv&bedrooms=2";
  const res = getProperties(queryParams);

  check(res, {
    "GET /api/properties is 200": (r) => r.status === 200,
  });

  sleep(1);
}
