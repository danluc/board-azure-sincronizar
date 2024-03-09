import { Component, OnInit, ViewChild } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";
import { DashboardComponent } from "app/dashboard/dashboard.component";

declare const $: any;
declare interface RouteInfo {
  path: string;
  title: string;
  icon: string;
  class: string;
}
export const ROUTES: RouteInfo[] = [
  { path: "/dashboard", title: "Sincronizar", icon: "dashboard", class: "" },
  { path: "/contas", title: "Contas", icon: "groups", class: "" },
  { path: "/azure", title: "Azure", icon: "apps", class: "" },
  { path: "/configuracoes", title: "Configurações", icon: "settings", class: "" },
];
declare var electron: any;

@Component({
  selector: "app-sidebar",
  templateUrl: "./sidebar.component.html",
  styleUrls: ["./sidebar.component.css"],
})
export class SidebarComponent implements OnInit {
  menuItems: any[];
  public loading = false;
  @ViewChild(DashboardComponent)
  private _dashboardComponent: DashboardComponent;
  constructor(private _snackBar: MatSnackBar) {}

  ngOnInit() {
    this.menuItems = ROUTES.filter((menuItem) => menuItem);
    try {
      electron.ipcRenderer.on("SincronizacaoInicio", (event, mensagem) => {
        this._sincronizarInicio();
      });
      electron.ipcRenderer.on("SincronizacaoFim", (event, mensagem) => {
        this._sincronizarFim();
      });
    } catch (error) {
      console.log("websocketService", error);
    }
  }
  isMobileMenu() {
    if ($(window).width() > 991) {
      return false;
    }
    return true;
  }

  private _sincronizarInicio(): void {
    document.getElementById("listar-sincronizar-board")?.click();
    this.loading = true;
    this._snackBar.open("Sincronização iniciada!", "Ok", {
      duration: 3000,
    });
  }

  private _sincronizarFim(): void {
    document.getElementById("listar-sincronizar-board")?.click();
    this.loading = false;
    this._snackBar.open("Sincronização realizada!", "Ok", {
      duration: 4000,
    });
  }
}
