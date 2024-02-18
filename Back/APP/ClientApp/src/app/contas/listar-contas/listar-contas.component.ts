import { Component, OnInit } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Router } from "@angular/router";
import { ContasControllerService } from "app/core/services/ContasController.service";

@Component({
  selector: "app-listar-contas",
  templateUrl: "./listar-contas.component.html",
  styleUrls: ["./listar-contas.component.scss"],
})
export class ListarContasComponent implements OnInit {
  public carregando: boolean = false;
  constructor(private _contasControllerService: ContasControllerService, private _router: Router, private _snackBar: MatSnackBar) {}

  ngOnInit() {
    this._buscarContas();
  }

  private _buscarContas(): void {
    this.carregando = true;
    this._contasControllerService.lista().subscribe(
      (res) => {
        this.carregando = false;
        if (res.dados.length == 0) {
          this._router.navigate(["/contas/add"]);
          return;
        }
      },
      (erro) => {
        this.carregando = false;
        console.log(erro);
      }
    );
  }
}
