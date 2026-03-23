import ky from "ky";

let redirecting = false;

export const api = ky.create({
  hooks: {
    afterResponse: [
      (_request, _options, response) => {
        if (response.status === 401 && !redirecting) {
          redirecting = true;
          sessionStorage.removeItem("staffToken");
          window.location.href = "/";
        }
      },
    ],
  },
});
