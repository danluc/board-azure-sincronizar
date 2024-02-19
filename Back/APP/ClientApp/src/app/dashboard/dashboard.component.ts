import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { ContasControllerService } from "app/core/services/ContasController.service";
import { MatSnackBar } from "@angular/material/snack-bar";
import { SincronizarControllerService } from "app/core/services/SincronizarController.service";

@Component({
  selector: "app-dashboard",
  templateUrl: "./dashboard.component.html",
  styleUrls: ["./dashboard.component.scss"],
})
export class DashboardComponent implements OnInit {
  public carregando: boolean = false;
  constructor(
    private _contasControllerService: ContasControllerService,
    private _sincronizarControllerService: SincronizarControllerService,
    private _router: Router,
    private _snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this._buscarContas();
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

  public async sincronizar(): Promise<void> {
    this.carregando = true;
    try {
      const res = await this._sincronizarControllerService.sincronizar().toPromise();
      this.carregando = false;
      this._snackBar.open("Sincronização realizada com sucesso!", "Ok", {
        duration: 4000,
      });
    } catch (error) {
      this._snackBar.open("Erro ao sincronizar boards", "Fechar", {
        duration: 3000,
      });
      this.carregando = false;
    }
  }
}
