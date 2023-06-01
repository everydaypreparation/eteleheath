import { Injectable } from "@angular/core";

@Injectable()
export class PropConfig {
    readonly tokenKey = "app-tn";
    readonly roles = ["ADMIN", "CONSULTANT", "PATIENT",  "FAMILYDOCTOR", "INSURANCE", "MEDICALLEGAL", "DIAGNOSTIC"];
    readonly defaultTimezoneId = "America/Toronto";
}
