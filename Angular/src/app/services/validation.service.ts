import { Injectable } from "@angular/core";

@Injectable()
export class ValidationService {
  obj = {
    value: undefined,
    next: true,
    message: "",
    error: false,
    exception: false,
  };
  setValue(val: any): this {
    this.obj = JSON.parse(JSON.stringify(this.obj));
    this.obj.next = true;
    this.obj.message = "";
    this.obj.value = val;
    return this;
  };
  required(msg: string): this {
    if (this.isEmpty(this.obj.value)) {
      this.setError(msg);
    }
    return this;
  };
  optional(): this {
    if (this.isEmpty(this.obj.value)) {
      this.setError("");
    }
    return this;
  };
  /* string */
  isString(msg: string): this {
    if (this.obj.next) {
      if (typeof this.obj.value != "string") {
        this.obj.value = this.makeString(this.obj.value);
        if (!this.obj.value) {
          this.setError(msg);
        }
      } else {
        this.obj.value = this.obj.value.trim();
      }
    }
    return this;
  };
  minLength(length: number, msg: string): this {
    if (this.obj.next) {
      if (this.obj.value.length < length) {
        this.setError(msg);
      }
    }
    return this;
  };
  maxLength(length: number, msg: string): this {
    if (this.obj.next) {
      if (this.obj.value.length > length) {
        this.setError(msg);
      }
    }
    return this;
  };
  rangeLength(range: any, msg: string): this {
    if (this.obj.next) {
      if (
        this.obj.value.length < range[0] ||
        this.obj.value.length > range[1]
      ) {
        this.setError(msg);
      }
    }
    return this;
  };
  alphabets(msg: string): this {
    if (this.obj.next) {
      if (!this.testRegex("^[a-zA-Z ]+$", this.obj.value)) {
        this.setError(msg);
      }
    }
    return this;
  };
  alphaNumeric(msg: string): this {
    if (this.obj.next) {
      if (!this.testRegex("^[a-zA-Z0-9]+$", this.obj.value)) {
        this.setError(msg);
      }
    }
    return this;
  };
  noSpecialChar(msg: string): this {
    if (this.obj.next) {
      if (!this.testRegex("^[a-zA-Z0-9\\.-@]+$", this.obj.value)) {
        this.setError(msg);
      }
    }
    return this;
  };
  isMail(msg: string): this {
    if (this.obj.next) {
      if (this.isValidMail(this.obj.value)) {
        this.obj.value = this.obj.value.toLowerCase();
      } else {
        this.setError(msg);
      }
    }
    return this;
  };
  isValidMail(mail: string): boolean {
    return this.testRegex(
      "^\\w+([\\.-]?\\w+)*@\\w+([\\.-]?\\w+)*(\\.\\w{2,3})+$",
      mail
    );
  };
  /* number */
  isInt(msg: string): this {
    if (this.obj.next) {
      this.obj.value = this.makeInt(this.obj.value);
      if (this.obj.value == null) {
        this.setError(msg);
      }
    }
    return this;
  };
  isFloat(msg: string, fx: number): this {
    if (this.obj.next) {
      this.obj.value = this.makeFloat(this.obj.value, fx);
      if (this.obj.value == null) {
        this.setError(msg);
      }
    }
    return this;
  };
  isNumber(msg: string): this {
    if (this.obj.next) {
      this.obj.value = this.makeNumber(this.obj.value);
      if (this.obj.value == null) {
        this.setError(msg);
      }
    }
    return this;
  };
  min(min: number, msg: string): this {
    if (this.obj.next) {
      if (this.obj.value < Number(min)) {
        this.setError(msg);
      }
    }
    return this;
  };
  max(max: number, msg: string): this {
    if (this.obj.next) {
      if (this.obj.value > Number(max)) {
        this.setError(msg);
      }
    }
    return this;
  };
  range(range: any, msg: string): this {
    if (this.obj.next) {
      if (
        this.obj.value < Number(range[0]) ||
        this.obj.value > Number(range[1])
      ) {
        this.setError(msg);
      }
    }
    return this;
  };
  length(length: number, msg: string): this {
    if (this.obj.next) {
      let val = this.obj.value + "";
      if (val.toString().length != Number(length)) {
        this.setError(msg);
      }
    }
    return this;
  };
  /* boolean */
  isBoolean(msg: string): this {
    if (this.obj.next) {
      if (typeof this.obj.value != "boolean") {
        this.obj.value = this.makeBoolean(this.obj.value);
        if (this.obj.value == null) {
          this.setError(msg);
        }
      }
    }
    return this;
  };
  /* date */
  isDate(msg: string): this {
    if (this.obj.next) {
      if (!this.isValidDate(this.obj.value)) {
        this.obj.value = this.makeDate(this.obj.value);
        if (this.obj.value == null) {
          this.setError(msg);
        }
      }
    }
    return this;
  };
  isTime(msg: string): this {
    if (this.obj.next) {
      this.obj.value = this.makeString(this.obj.value);
      if (!this.isValidTime(this.obj.value)) {
        this.setError(msg);
      }
    }
    return this;
  };
  minDate(min: any, msg: string): this {
    if (this.obj.next) {
      if (this.obj.value.getTime() < new Date(min).getTime()) {
        this.setError(msg);
      }
    }
    return this;
  };
  maxDate(max: any, msg: string): this {
    if (this.obj.next) {
      if (this.obj.value.getTime() > new Date(max).getTime()) {
        this.setError(msg);
      }
    }
    return this;
  };
  rangeDate(range: any, msg: any): this {
    if (this.obj.next) {
      if (
        this.obj.value.getTime() < new Date(range[0]).getTime() ||
        this.obj.value.getTime() > new Date(range[1]).getTime()
      ) {
        this.setError(msg);
      }
    }
    return this;
  };
  /* other methods */
  isEmpty(val: any): boolean {
    if (val === "" || val === null || val === undefined) {
      return true;
    }
    return false;
  };
  regex(exp: any, msg: string) {
    if (this.obj.next) {
      if (!this.testRegex(exp, this.obj.value)) {
        this.setError(msg);
      }
    }
    return this;
  }
  testRegex(rgx: any, val: any): boolean {
    return new RegExp(rgx).test(val);
  };
  isValidDate(date: any): boolean {
    if (date == "Invalid Date") {
      return false;
    }
    if (Object.prototype.toString.call(date) === "[object Date]") {
      if (isNaN(date.getTime())) {
        return false;
      } else {
        return true;
      }
    } else {
      return false;
    }
  };
  isValidTime(time: string): boolean {
    return this.testRegex(
      "^([0-1]?[0-9]|2[0-3]):([0-5][0-9])(:[0-5][0-9])?$",
      time);
  };
  isValidJSON(val: any): boolean {
    try {
      if (typeof val != "string") {
        val = JSON.stringify(val);
      }
      val = JSON.parse(val);
      if (val == undefined || typeof val != "object") {
        return false;
      } else {
        return true;
      }
    } catch (e) {
      return false;
    }
  };
  makeString(val: any): string {
    if (val != undefined && val != null) {
      val = val + "";
      val = val.trim();
      return val.toString();
    } else {
      return val;
    }
  };
  makeInt(val: any): any {
    if (val != undefined && val != null && val != "") {
      val = this.makeString(val);
      if (Number(val) == val && this.testRegex(/^\d+$/, Number(val))) {
        return parseInt(val);
      } else {
        return null;
      }
    } else {
      return null;
    }
  };
  makeFloat(val: any, fx: number): any {
    if (val != undefined && val != null && val != "") {
      val = this.makeString(val);
      if (Number(val) == val) {
        if (fx) {
          return Number(parseFloat(val).toFixed(fx));
        }
        return Number(val);
      } else {
        return null;
      }
    } else {
      return null;
    }
  };
  makeNumber(val: any): any {
    if (val != undefined && val != null && val != "") {
      val = this.makeString(val);
      if (Number(val) == val && this.testRegex(/^\d+$/, Number(val))) {
        return val;
      } else {
        return null;
      }
    } else {
      return null;
    }
  };
  makeBoolean(val: any): any {
    if (val != undefined && val != null && val != "") {
      val = this.makeString(val).toLowerCase();
      if (val == "true" || val == true || val == "yes" || val == "1") {
        return true;
      } else if (val == "false" || val == false || val == "no" || val == "0") {
        return false;
      } else {
        return null;
      }
    } else {
      return null;
    }
  };
  makeDate(date: any): any {
    if (date != undefined && date != null && date != "") {
      date = this.makeString(date);
      date = new Date(date);
      if (date == "Invalid Date") {
        return null;
      }
      if (Object.prototype.toString.call(date) === "[object Date]") {
        if (isNaN(date.getTime())) {
          return null;
        } else {
          return date;
        }
      } else {
        return null;
      }
    } else {
      return null;
    }
  };
  getValue(obj: any): any {
    return Object.assign({}, obj);
  };
  findError(objs: any): any {
    let obj = null;
    Object.keys(objs).map((key, index) => {
      if (obj == null && objs[key].message != "") {
        obj = objs[key];
        obj.key = key;
      }
    });
    return obj;
  };
  setError(msg: string): void {
    this.obj.next = false;
    this.obj.error = true;
    this.obj.message = msg;
  };
};