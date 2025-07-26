/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: "#FF6B00",     // turuncu - butonlar
          light: "#FFA756",     // açık turuncu - hover
          dark: "#E85D00",     // koyu turuncu
        },
        background: "#FFFFFF",     // arka plan beyaz
        surface: "#F8F8F8",        // kartların arka planı
        accent: "#111827",         // çok koyu gri - başlıklar
        text: "#374151",           // içerik metinleri
        danger: "#DC2626",
        success: "#16A34A",
      }
    },
  },
  plugins: [
    require("@tailwindcss/forms"),
    require("@tailwindcss/typography"),
  ],
};
