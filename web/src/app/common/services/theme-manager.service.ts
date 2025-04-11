import { Injectable } from "@angular/core";

@Injectable({
    providedIn: "root"
})
export class ThemeManagerService {
    private _root: HTMLElement = document.querySelector(":root") as HTMLElement;

    constructor() { }

    laodThemeCustomization(): void {
        this.loadColorCustomization();
        this.loadBackgroundCustomization()
    }

    loadColorCustomization(value?: string): void {
        let primaryHue: string = '252';
        const storeValue = value ?? localStorage.getItem('custom-color');

        switch (storeValue) {
            case '1':
                primaryHue = '252';
                break;
            case '2':
                primaryHue = '42';
                break;
            case '3':
                primaryHue = '352';
                break;
            case '4':
                primaryHue = '120';
                break;
            case '5':
                primaryHue = '210';
                break;
            default:
                break;
        }

        this._root.style.setProperty('--primary-hue', primaryHue);
    }

    loadBackgroundCustomization(value?: string): void {
        let lightColorLightness: string = '95%';
        let whiteColorLightness: string = '100%';
        let darkColorLightness: string = '17%';

        const storeValue = value ?? localStorage.getItem('custom-bg');

        switch (storeValue) {
            case '1':
                darkColorLightness = '17%';
                whiteColorLightness = '100%';
                lightColorLightness = '95%';
                break;
            case '2':
                darkColorLightness = '95%';
                whiteColorLightness = '20%';
                lightColorLightness = '15%';
                break;
            case '3':
                darkColorLightness = '95%';
                whiteColorLightness = '10%';
                lightColorLightness = '0%';
                break;
            default:
                break;
        }

        this._root.style.setProperty('--light-color-lightness', lightColorLightness);
        this._root.style.setProperty('--white-color-lightness', whiteColorLightness);
        this._root.style.setProperty('--dark-color-lightness', darkColorLightness);
    }
}