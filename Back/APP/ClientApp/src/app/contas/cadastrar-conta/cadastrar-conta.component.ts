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
  public contaAtualizar: Conta;
  public contas: Conta[] = [];
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
      area: ["", [Validators.required]],
      sprint: ["", [Validators.required]],
      cliente: ["", [Validators.required]],
    });
  }

  private async _buscarContas(): Promise<void> {
    this.carregando = true;
    const res = await this._contasControllerService.lista().toPromise();
    this.contas = res;
    this.carregando = false;
  }

  public async buscarAreas(): Promise<void> {
    this.carregando = true;
    this.textoCarregando = "Buscando areas...";

    try {
      const res = await this._timesControllerService.listaAreas().toPromise();
      this.carregando = false;
      this.areas = res;
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
      this.sprints = res;
    } catch (error) {
      this.carregando = false;
    }
  }

  private get _montarObj(): Conta {
    let conta: Conta = {
      emailDe: this.form.get("emailDe").value,
      emailPara: this.form.get("emailPara").value,
      areaId: this.form.get("area").value?.id,
      areaPath: this.form.get("area").value?.path,
      cliente: this.form.get("cliente").value,
      sprint: this.form.get("sprint").value?.path,
      sprintId: this.form.get("sprint").value?.id,
    };

    return conta;
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
      const res = await this._contasControllerService.cadastrar(contas).toPromise();
      this.carregando = false;
      this._buscarContas();
      this.cancelarEditar();
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
      contas.id = this.contaAtualizar.id;
      const res = await this._contasControllerService.atualizar(contas).toPromise();
      this.carregando = false;
      this._buscarContas();
      this.cancelarEditar();
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

  public editar(conta: Conta): void {
    let area = this.areas.find((e) => e.id == conta.areaId);
    let sprints = this.sprints.filter((e) => e.sprints.find((e) => e.id == conta.sprintId)).map((e) => e.sprints)[0];
    let sprint = sprints.find((e) => e.id == conta.sprintId);
    this.form.get("emailDe").setValue(conta.emailDe);
    this.form.get("emailPara").setValue(conta.emailPara);
    this.form.get("area").setValue(area);
    this.form.get("sprint").setValue(sprint);
    this.form.get("cliente").setValue(conta.cliente);
    this.cadastrar = false;
    this.contaAtualizar = conta;
  }

  public cancelarEditar(): void {
    this.cadastrar = true;
    this.contaAtualizar = null;
    this.form.reset();
  }
}
