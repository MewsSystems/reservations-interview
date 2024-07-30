/** @type {import('tailwindcss'.Config)} */
module.exports = {
  content: [
    "./src/**/*.{html,tsx}",
    "./node_modules/react-tailwindcss-datepicker/dist/index.esm.js",
  ],
  theme: {
    extend: {},
  },
  plugins: [require("@tailwindcss/typography"), require("daisyui")],
};
