import { Component, OnInit } from '@angular/core';
import { ThemeManagerService } from '../../common/services/theme-manager.service';

@Component({
  selector: 'app-customize-theme-dialog',
  standalone: true,
  imports: [],
  templateUrl: './customize-theme-dialog.component.html',
  styleUrl: './customize-theme-dialog.component.scss'
})
export class CustomizeThemeDialogComponent implements OnInit {
  constructor(private themeManagerSvc: ThemeManagerService) { }

  ngOnInit(): void {
    this.initColorCustomization();
    this.initBackgroundCustomization();
  }

  initColorCustomization(): void {
    const colorPalette = document.querySelectorAll('.choose-color span');

    const resetActiveColorClass = (setDefault?: boolean) => {
      const storedColorValue = localStorage.getItem('custom-color') ?? '1';
      colorPalette.forEach(color => {
        color.classList.remove('active');

        if (setDefault && color.classList.contains(`color-${storedColorValue}`)) {
          color.classList.add('active');
        }
      });
    };

    resetActiveColorClass(true);

    colorPalette.forEach(color => {
      color.addEventListener('click', () => {
        resetActiveColorClass();
        color.classList.add('active');

        let selectedColorValue: string = localStorage.getItem('custom-color') ?? '1';

        if (color.classList.contains('color-1')) {
          selectedColorValue = '1';
        } else if (color.classList.contains('color-2')) {
          selectedColorValue = '2';
        } else if (color.classList.contains('color-3')) {
          selectedColorValue = '3';
        } else if (color.classList.contains('color-4')) {
          selectedColorValue = '4';
        } else if (color.classList.contains('color-5')) {
          selectedColorValue = '5';
        }

        localStorage.setItem('custom-color', selectedColorValue);
        this.themeManagerSvc.loadColorCustomization(selectedColorValue);
      });
    });
  }

  initBackgroundCustomization(): void {
    const bgPalette = document.querySelectorAll('.choose-bg div');

    const resetActiveBgClass = (setDefault?: boolean) => {
      const storedBgValue = localStorage.getItem('custom-bg') ?? '1';
      bgPalette.forEach(bg => {
        bg.classList.remove('active');

        if (setDefault && bg.classList.contains(`bg-${storedBgValue}`)) {
          bg.classList.add('active');
        }
      });
    }

    resetActiveBgClass(true);

    bgPalette.forEach(bg => {
      bg.addEventListener('click', () => {
        resetActiveBgClass();
        bg.classList.add('active');

        let selectedBgValue: string = localStorage.getItem('custom-bg') ?? '1';
        if (bg.classList.contains('bg-1')) {
          selectedBgValue = '1';
        }
        else if (bg.classList.contains('bg-2')) {
          selectedBgValue = '2';
        } else if (bg.classList.contains('bg-3')) {
          selectedBgValue = '3';
        }

        localStorage.setItem('custom-bg', selectedBgValue);
        this.themeManagerSvc.loadBackgroundCustomization(selectedBgValue);
      });
    });
  }
}
