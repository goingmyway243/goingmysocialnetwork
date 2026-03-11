import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';

const MyPreset = definePreset(Aura, {
  semantic: {
    colorScheme: {
      light: {
        primary: {
          hoverColor: '#ffffff',
          activeColor: '#ffffff',
        },
        highlight: {
          focusColor: '#ffffff',
        }
      }
    }
  }
});

export default MyPreset;