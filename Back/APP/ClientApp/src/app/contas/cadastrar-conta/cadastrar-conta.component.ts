import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { MatSnackBar } from "@angular/material/snack-bar";
import { Router } from "@angular/router";
import { Conta } from "app/core/models/conta";
import { ListaSprintsDTO } from "app/core/models/lista-sprintsDTO";
import { TeamProjectReference } from "app/core/models/team-project-reference";
import { ContasControllerService } from "app/core/services/ContasController.service";
import { TimesControllerService } from "app/core/services/TimesController.service";

@Component({
  selector: "app-cadastrar-conta",
  templateUrl: "./cadastrar-conta.component.html",
  styleUrls: ["./cadastrar-conta.component.scss"],
})
export class CadastrarContaComponent implements OnInit {
  public cadastrar: boolean = true;
  public carregando: boolean = false;
  public form: FormGroup;
  public projetosPrincipal: TeamProjectReference[] = [];
  public projetosSecundario: TeamProjectReference[] = [];
  public times: TeamProjectReference[] = [];
  public areas: TeamProjectReference[] = [];
  public sprints: ListaSprintsDTO[] = [];
  public contaAtualizar: Conta[] = [];
  public textoCarregando: string = "Buscando contas...";

  constructor(
    private _contasControllerService: ContasControllerService,
    private _timesControllerService: TimesControllerService,
    private _formBuilder: FormBuilder,
    private _router: Router,
    private _snackBar: MatSnackBar
  ) {}

  ngOnInit() {
    this._formularioCad();
    this._buscarContas();
    this.buscarAreas();
    this.buscarSprint();
  }

  private _formularioCad(): void {
    this.form = this._formBuilder.group({
      emailDe: ["", [Validators.required]],
      emailPara: ["", [Validators.required]],
      areaId: ["", [Validators.required]],
      areaPath: ["", [Validators.required]],
      sprint: ["", [Validators.required]],
      cliente: ["", [Validators.required]],
    });
  }

  private async _buscarContas(): Promise<void> {
    this.carregando = true;
    const res = await this._contasControllerService.lista().toPromise();
    console.log(res);
  }

  public async buscarAreas(): Promise<void> {
    this.carregando = true;
    this.textoCarregando = "Buscando areas...";

    try {
      const res = await this._timesControllerService.listaAreas().toPromise();
      this.carregando = false;
      this.areas = res.dados;
      console.log(res);
    } catch (error) {
      this.carregando = false;
      this._snackBar.open("Problema para acessar a azure com as credenciais", "Fechar", {
        duration: 3000,
      });
      this._router.navigate(["/azure"]);
    }
  }

  public async buscarSprint(): Promise<void> {
    this.carregando = true;
    this.textoCarregando = "Buscando sprint...";
    this.sprints = [];
    try {
      const res = await this._timesControllerService.listaSprint().toPromise();
      this.carregando = false;
      this.sprints = res.dados;
      console.log(res);
    } catch (error) {
      this.carregando = false;
    }
  }

  private get _montarObj(): Conta[] {
    let contas: Conta[] = [
      {
        /* urlCorporacao: this.form.get("urlPrincipal").value,
        token: this.form.get("tokenPrincipal").value,
        projetoId: this.form.get("projetoPrincipal").value?.id,
        projetoNome: this.form.get("projetoPrincipal").value?.name,
        principal: true,*/
      },
      {
        /*urlCorporacao: this.form.get("urlSecundario").value,
        token: this.form.get("tokenSecundario").value,
        areaPath: this.form.get("areaPath").value?.name,
        nomeUsuario: this.form.get("nomeSecundario").value,
        projetoId: this.form.get("projetoSecundario").value?.id,
        timeId: this.form.get("time").value?.id,
        projetoNome: this.form.get("projetoSecundario").value?.name,
        timeNome: this.form.get("time").value?.name,
        sprint: this.form.get("sprint").value?.path,
        principal: false,*/
      },
    ];
    return contas;
  }

  public async salvar(): Promise<void> {
    this.carregando = true;
    this.textoCarregando = "Salvando...";
    if (this.cadastrar) {
      await this._cadastrar();
    } else {
      await this._atualizar();
    }
  }

  private async _cadastrar(): Promise<void> {
    try {
      const contas = this._montarObj;
      const res = await this._contasControllerService.cadastrar({}).toPromise();
      this.carregando = false;
      this._snackBar.open("Contas cadastrada com sucesso!", "Fechar", {
        duration: 3000,
      });
    } catch (error) {
      this.carregando = false;
      this._snackBar.open("Erro para cadastrar conta!", "Fechar", {
        duration: 5000,
      });
    }
  }

  private async _atualizar(): Promise<void> {
    try {
      const contas = this._montarObj;
      const res = await this._contasControllerService.atualizar({}).toPromise();
      this.carregando = false;
      this._snackBar.open("Contas atualizadas com sucesso!", "Fechar", {
        duration: 3000,
      });
    } catch (error) {
      this.carregando = false;
      this._snackBar.open("Erro ao atualizar conta!", "Fechar", {
        duration: 5000,
      });
    }
  }
}
