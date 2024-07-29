/** Make a POST to an endpoint with some JSON
 *
 * @param url
 * @param json
 * @returns
 */
export function postJson<TResultData>(url: string, json: any) {
  return fetch(url, {
    method: "POST",
    // TODO handle serialization better
    body: JSON.stringify(json),
    headers: {
      "Content-Type": "application/json",
    },
  }).then((response) => {
    // TODO handle errors better
    // TODO handle types better
    return response.json() as TResultData;
  });
}

/** Make a GET request to get back some JSON
 *
 * @param url
 * @returns
 */
export function getJson<TResultData>(url: string) {
  return fetch(url).then((response) => {
    // TODO handle errors better
    // TODO handle types better
    return response.json() as TResultData;
  });
}
