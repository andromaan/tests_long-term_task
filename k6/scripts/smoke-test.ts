import { check, sleep } from "k6";
import { Options } from "k6/options";
import { THRESHOLDS } from "../helpers/config.ts";
import { getProperties } from "../helpers/api-client.ts";

export const options: Options = {
  vus: 1,
  duration: "20s",
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
