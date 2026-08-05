/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      borderRadius: {
        '4xl': '2rem', 
        '5xl': '3rem',
      },
      boxShadow: {
        'antigravity': '0 20px 40px -10px rgba(0,0,0,0.08), 0 1px 3px rgba(0,0,0,0.05)', 
        'antigravity-hover': '0 30px 60px -12px rgba(0,0,0,0.12), 0 4px 8px rgba(0,0,0,0.04)', 
        'antigravity-inner': 'inset 0 2px 4px 0 rgba(255, 255, 255, 0.3)', 
      },
      colors: {
        background: '#f3f4f6', 
        glass: 'rgba(255, 255, 255, 0.65)', 
        glassBorder: 'rgba(255, 255, 255, 0.4)',
        primary: '#4f46e5', 
      },
      backdropFilter: {
        'none': 'none',
        'blur': 'blur(16px)', 
      },
    },
  },
  plugins: [],
}