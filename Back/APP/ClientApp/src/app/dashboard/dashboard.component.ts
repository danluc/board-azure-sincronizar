import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { ContasControllerService } from "app/core/services/ContasController.service";
import { MatSnackBar } from "@angular/material/snack-bar";
import { SincronizarControllerService } from "app/core/services/SincronizarController.service";
import { Sincronizar } from "app/core/models/sincronizar";

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.scss"],
})
export class DashboardComponent implements OnInit {
  public singnalConectado: boolean = sessionStorage.getItem("SingnalConectado") == "true";
  public carregando: boolean = false;
  public mostrarItens: boolean = false;
  public sincronizarDTO: Sincronizar;
  constructor(
    private _contasControllerService: ContasControllerService,
    private _sincronizarControllerService: SincronizarControllerService,
    private _router: Router,
    private _snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this._buscarContas();
    this.listaSincronizar();
  }

  private _buscarContas(): void {
    this.carregando = true;
    this._contasControllerService.lista().subscribe(
      (res) => {
        this.carregando = false;
        if (res.dados.length == 0) {
          this._router.navigate(["/contas"]);
          this._snackBar.open("Nenhuma conta encontrada!", "Fechar", {
            duration: 3000,
          });
          return;
        }
      },
      (erro) => {
        this.carregando = false;
        console.log(erro);
      }
    );
  }

  public async listaSincronizar(): Promise<void> {
    try {
      const res = await this._sincronizarControllerService.ultimo().toPromise();
      if (res != null) {
        this.sincronizarDTO = res;
        if (this.sincronizarDTO.status == 1) {
          this.carregando = true;
        } else {
          this.carregando = false;
        }
      }
    } catch (error) {
      this._snackBar.open("Erro ao listar", "Fechar", {
        duration: 3000,
      });
      this.carregando = false;
    }
  }

  public async sincronizar(): Promise<void> {
    this.carregando = true;
    try {
      setTimeout(() => {
        this.listaSincronizar();
      }, 100);
      const res = await this._sincronizarControllerService.sincronizar().toPromise();
      this.listaSincronizar();
      this.carregando = false;

      if (!this.singnalConectado) {
        this._snackBar.open("Sincronização realizada!", "Ok", {
          duration: 4000,
        });
      }
    } catch (error) {
      this._snackBar.open("Erro ao sincronizar boards", "Fechar", {
        duration: 3000,
      });
      this.carregando = false;
      this.listaSincronizar();
    }
  }

  public alterarLista(): void {
    this.mostrarItens = !this.mostrarItens;
  }
}
