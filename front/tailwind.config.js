/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        bm: {
          bg: '#0F1419',
          sidebar: '#161B22',
          surface: '#1C2128',
          'surface-2': '#22272E',
          border: '#2A3038',
          text: '#E6EDF3',
          muted: '#8B949E',
          cyan: '#2EC5FF',
          'cyan-dim': '#1FA8DC',
          success: '#3FB950',
          danger: '#F85149',
          warning: '#F0883E'
        }
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'Segoe UI', 'sans-serif']
      },
      boxShadow: {
        glow: '0 0 0 1px rgba(46, 197, 255, 0.3), 0 0 20px rgba(46, 197, 255, 0.15)'
      }
    }
  },
  plugins: []
};
