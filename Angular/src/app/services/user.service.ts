import { Injectable, Output, EventEmitter } from "@angular/core";
import { PropConfig } from "../configs/prop.config";

@Injectable()
export class UserService {

  @Output() changeUser: EventEmitter<any> = new EventEmitter<any>();
  @Output() changeNav: EventEmitter<any> = new EventEmitter<any>();
  @Output() toggleMobileNav: EventEmitter<any> = new EventEmitter<any>();
  @Output() toggleSamvaadNav: EventEmitter<any> = new EventEmitter<any>();
  @Output() toggleSamvaadNavIcon: EventEmitter<any> = new EventEmitter<any>();

  private isAuthenticated: boolean = false;
  public userRole: string = "";
  private user: any = null;

  constructor(private propConfig: PropConfig,) {
    this.isAuthenticated = false;
  }

  setNav(nav: any) {
    this.changeNav.emit(nav);
  }

  toggleMobileNavClick(): void {
    this.toggleMobileNav.emit({});
  }

  toggleSamvaadNavIconClick(data: any): void {
    this.toggleSamvaadNavIcon.emit(data);
  }

  toggleSamvaadNavClick(): void {
    this.toggleSamvaadNav.emit({});
  }

  setUser(user: any): void {
    this.user = user;
    this.user.id = user.uniqueUserId;
    this.userRole = user.roleNames[0];
    // this.userRole = localStorage.getItem("role");
    this.changeUser.emit({ "data": user });
    /*
    subscribeUser: any;
    this.subscribeUser = this.userService.changeUser.subscribe((data: any) => {      
    });
    if (this.subscribeUser != null) {
      this.subscribeUser.unsubscribe();
    }
    */
  }

  getUser(): any {
    return this.user;
  }

  setToken(token: string): void {
    try {
      localStorage.setItem(this.propConfig.tokenKey, token);
    } catch (e) {
      console.log("Error Exception UserService setToken " + e);
    }
  }

  getToken(): string {
    let tkn = "";
    try {
      tkn = localStorage.getItem(this.propConfig.tokenKey);
    } catch (e) {
      console.log("Error Exception UserService getToken " + e);
    }
    return tkn;
  }

  hasRole(roles: any): boolean {
    if (roles && roles.length > 0) {
      return roles.includes(this.userRole);
    }
    return false;
  }

  isUserAuthenticated(): boolean {
    try {
      this.isAuthenticated = false;
      const tkn = localStorage.getItem(this.propConfig.tokenKey);
      if (!(tkn === "" || tkn === null || tkn === undefined)) {
        this.isAuthenticated = true;
      }
    } catch (e) {
      console.log("Error Exception UserService isUserAuthenticated " + e);
    }
    return this.isAuthenticated;
  }

  clearUserData(): boolean {
    try {

      if (this.user.roleNames[0] == "ADMIN") {
        localStorage.removeItem("Impersonator");
        localStorage.removeItem("IsImpersonating");
      }

      this.isAuthenticated = false;
      // localStorage.clear();
      localStorage.removeItem("userId");
      localStorage.removeItem(this.propConfig.tokenKey);
    } catch (e) {
      console.log("Error Exception UserService clearUserData " + e);
    }
    return true;
  }
}
