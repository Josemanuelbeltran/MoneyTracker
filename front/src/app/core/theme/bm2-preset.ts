import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

export const Bm2Preset = definePreset(Aura, {
  semantic: {
    primary: {
      50: '#E5F8FF',
      100: '#BFEDFF',
      200: '#8DDFFF',
      300: '#5AD0FF',
      400: '#37C9FF',
      500: '#2EC5FF',
      600: '#1FA8DC',
      700: '#1788B3',
      800: '#10688A',
      900: '#0A4861',
      950: '#062F40',
    },
    colorScheme: {
      dark: {
        surface: {
          0: '#FFFFFF',
          50: '#E6EDF3',
          100: '#C9D1D9',
          200: '#8B949E',
          300: '#6E7681',
          400: '#484F58',
          500: '#3A414B',
          600: '#2A3038',
          700: '#22272E',
          800: '#1C2128',
          900: '#161B22',
          950: '#0F1419',
        },
        primary: {
          color: '{primary.500}',
          contrastColor: '#0F1419',
          hoverColor: '{primary.400}',
          activeColor: '{primary.600}',
        },
        highlight: {
          background: 'rgba(46, 197, 255, 0.16)',
          focusBackground: 'rgba(46, 197, 255, 0.24)',
          color: '{primary.300}',
          focusColor: '{primary.200}',
        },
        formField: {
          background: '#1C2128',
          disabledBackground: '#161B22',
          filledBackground: '#22272E',
          filledHoverBackground: '#2A3038',
          filledFocusBackground: '#22272E',
          borderColor: '#2A3038',
          hoverBorderColor: '#3A414B',
          focusBorderColor: '{primary.500}',
          invalidBorderColor: '#F85149',
          color: '#E6EDF3',
          disabledColor: '#6E7681',
          placeholderColor: '#6E7681',
          floatLabelColor: '#8B949E',
          floatLabelFocusColor: '{primary.500}',
          floatLabelActiveColor: '#8B949E',
          floatLabelInvalidColor: '#F85149',
          iconColor: '#8B949E',
        },
      },
    },
  },
});
