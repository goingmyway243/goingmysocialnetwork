import { definePreset } from '@primeuix/themes';
import Lara from '@primeuix/themes/lara';

const MyPreset = definePreset(Lara, {
  semantic: {
    colorScheme: {
      light: {
        primary: {
          color: 'rgba(255, 255, 255, 0.1)',
          hoverColor: 'rgba(255, 255, 255, 0.2)',
          activeColor: 'rgba(255, 255, 255, 0.15)'
        }
      },
      dark: {
        primary: {
          color: 'rgba(0, 0, 0, 0.6)',
        }
      }
    }
  }
});

export default MyPreset;