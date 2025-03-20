import { HttpErrorResponse } from "@angular/common/http";
import { Content } from "../models/content.model";
import { environment } from "../../../environments/environment";

export class Util {
    static getHttpErrorMessage(httpError: HttpErrorResponse): string {
        let message = httpError.error;
        return message.substring(message.indexOf(': ') + 1, message.indexOf('\r'));
    }

    static padTo2Digits(num: number): string {
        return num.toString().padStart(2, '0');
    }

    static formatDate(date: Date): string {
        let values = date.toString().slice(0, 10).split('-');
        values.reverse();

        return [...values].join('/');
    }

    static getTimeDiff(time: Date): string {
        let now = new Date();
        let date = new Date(time.valueOf());
        let diff = Math.round((now.getTime() - date.getTime()) / 60000);

        if (diff < 1) {
            return 'Just now';
        } else if (diff < 60) {
            return diff + ' minutes ago';
        } else if (diff < 1440) {
            let hour = diff < 120 ? 'hour' : 'hours';
            return Math.floor(diff / 60) + ' ' + hour + ' ago';
        }
        else if (diff < 10080) {
            let day = diff < 2880 ? 'day' : 'days';
            return Math.floor(diff / 1440) + ' ' + day + ' ago';
        }
        else {
            return Util.formatDate(date);
        }
    }

    static getFullLinkContent(content: Content): string {
        return environment.baseUrl + 'app-images/' + content.postId + '/' + content.linkContent;
    }
}