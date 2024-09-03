import http from 'k6/http';
import { sleep } from 'k6';

export const options = {
  // Number of VUs (Virtual Users) to run concurrently
  vus: 10,
  // Duration of the test run
  duration: '30s',
};

export default function () {
  // Define the request payload
  const payload = JSON.stringify({
    roomNumber: "102",
    guestEmail: "ttosic@gmail.com",
    start: "2024-10-22T22:00:00.000Z",
    end: "2024-10-23T22:00:00.000Z",
  });

  // Define the request headers
  const headers = {
    'Content-Type': 'application/json',
  };

  // Make the POST request
  http.post('http://localhost:6001/api/reservation', payload, { headers });

  // Pause for 1 second
  sleep(1);
}
